namespace ConfiguationPersistence
open ConfiguationPersistence.ModelModule
open Configuration.Cryption
open System.IO
open System.Xml
open System.Xml.Linq
open System.Linq
open System.Collections.Generic
open System.Text.RegularExpressions
open System.Text

module StringsModule =
    let xmlns=XNamespace.Get "http://schemas.microsoft.com/.NetConfiguration/v2.0"
    let currentProgramName="ConfigurationManager";
    let connectionString="ConnectionString"
    let commonSetting="Common.xml"

module FileModule =

    let rec GetDirectorySize dir =
        Seq.append (dir |>Directory.GetFiles)
                (dir |> Directory.GetDirectories |> Seq.map GetDirectorySize |> Seq.concat)

    let GetSize dir =
        dir |> GetDirectorySize |> Seq.map (fun m -> 
            let file = File.Open(m,FileMode.Append)
            file.Length
            )
        |> Seq.sum

module UtilModule=

    let xn name =XName.op_Implicit name   

    let getSection (doc:XDocument) (fileInfo:ConfigFileInfo)=
        let col=doc.Element(XName.Get fileInfo.RootElement)
        match col with
        |null -> 
            let r=doc.Element(StringsModule.xmlns + fileInfo.RootElement)
            fileInfo.Sections|>Seq.map (fun m->( m,r.Element(StringsModule.xmlns + m.Name)))
        |_ -> fileInfo.Sections|>Seq.map (fun m->( m,col.Element(xn m.Name)))
   

    let ConvertMapToDictinary (map:Map<_,_>)=
        let dict= new Dictionary<_,_>()
        map|>Map.toSeq|>Seq.iter (fun (k,v)-> dict.Add(k,v))
        dict


    let getAttr (m:XElement) attr =
            match m.Attribute(xn attr) with
            |null ->""
            |x ->x.Value   


module ConfigModule =



    let rec GetConfigFile dir =
        let files = dir |> Directory.GetFiles |>Seq.filter (fun m -> Regex.IsMatch(m,"web.config",RegexOptions.IgnoreCase) && (not (Regex.IsMatch(m,StringsModule.currentProgramName)))) |>(fun m -> if (Seq.isEmpty m) then Seq.empty else Seq.ofList [(dir,(Seq.nth 0 m))])
        match Seq.isEmpty files with
        |false -> files
        |true ->dir |>Directory.GetDirectories|>Seq.map GetConfigFile|>Seq.concat

//return Seq of sectionName,nodeName,attributes,items
    let GetConfigContent (configFileInfo:ConfigFileInfo) =
        let root= XDocument.Load(configFileInfo.Path)
        let target= UtilModule.getSection root configFileInfo
        match target with
        |null -> Seq.empty
        |_ ->
        target|>Seq.map(fun (n,m)->(n,m.Descendants()))|>Seq.map (fun (n,m) ->
            m|>Seq.map (fun mm ->n.Node.Attrs|>Seq.map (fun mmm ->(mmm,(UtilModule.getAttr mm mmm)))|>Map.ofSeq|>UtilModule.ConvertMapToDictinary|>(fun dict ->
                let ok=dict.Values.ToArray()|>Array.filter (fun c-> Regex.IsMatch(c,StringsModule.commonSetting,RegexOptions.IgnoreCase))|>Seq.isEmpty
                match ok with
                |true -> 
                    match  dict.ContainsKey("value") with
                    |true -> dict.["value"] <- Decrypt (dict.["value"])
                    |false ->
                        match dict.ContainsKey("ConnectionString") with
                        |true -> dict.["ConnectionString"] <- Decrypt (dict.["ConnectionString"])
                        |false -> dict.["connectionString"] <- Decrypt (dict.["connectionString"])
                |false -> ()
                dict
                ) )|>(fun x -> (n.Name,n.Node.Name,n.Node.Attrs,x))
            )
    


    let GetConfigMappingFileContent (fileName:string) (sectionName:string) =
        let root =XElement.Load(fileName).Descendants(XName.Get sectionName)
        match Seq.isEmpty root with
        |true -> Seq.empty
        |false -> root.Single().Descendants()|>Seq.map (fun m-> ((UtilModule.getAttr m "key"),(UtilModule.getAttr m "value"),(UtilModule.getAttr m "type"),(UtilModule.getAttr m "values")))


    let private IsDirectoryContainsConfigFile dir =
        dir|> Directory.GetFiles|>Seq.filter (fun m -> m.IndexOf("Web.config")<> -1)|> Seq.isEmpty|>fun m -> not m

    let GetDirectoryShortName (dir:string) =
        dir|>(fun m -> m.Substring(m.LastIndexOf('\\')+1))

    let private getTarget (root:XElement) (sectionName:string) (isXmlns:bool) =
        match isXmlns with
        |true -> root.Descendants(StringsModule.xmlns + sectionName).Single()
        |false -> root.Descendants(UtilModule.xn sectionName).Single()

    let private DelConfigContent (root:XElement) (fileName:string) (section:string) (isXmlns:bool) =
        let target =getTarget root section isXmlns
        target.Descendants()|>List.ofSeq|>List.iter (fun m-> m.Remove())
        root.Save(fileName)

    let private save (fileName:string) (savedItem:SaveItem) (isXmlns:bool)=
        let root =XElement.Load fileName
        DelConfigContent root fileName savedItem.Section.Name isXmlns
        let root=XElement.Load(fileName)
        let appElement= getTarget root (savedItem.Section.Name) isXmlns
        let getNewElement() =
            match isXmlns with
            |true -> new XElement(StringsModule.xmlns + savedItem.Section.Node.Name)
            |false -> new XElement(UtilModule.xn savedItem.Section.Node.Name)
    
        let setAttrValue (xe:XElement) (key:string) (value:string)=
            match isXmlns with
            |true -> xe.SetAttributeValue((UtilModule.xn key),value)
            |false -> xe.SetAttributeValue((UtilModule.xn key),value)
    
        let ok=not(Regex.IsMatch(fileName,StringsModule.commonSetting,RegexOptions.IgnoreCase))
        for i in 0..Array2D.length1(savedItem.Items)|>(fun m->m - 1) do
            let target = getNewElement()
            setAttrValue target (savedItem.Section.Node.Attrs.[0]) (savedItem.Items.[i,0])
            match Regex.IsMatch(savedItem.Items.[i,0],StringsModule.connectionString,RegexOptions.IgnoreCase) && ok with
            |true -> setAttrValue target (savedItem.Section.Node.Attrs.[1]) (Encrypt (savedItem.Items.[i,1]))
            |false -> setAttrValue target (savedItem.Section.Node.Attrs.[1]) (savedItem.Items.[i,1])
            appElement.Add(target)
        root.Save(fileName)


    let private GetConfigContentFromStr (source:string) =
        let arr=source.Split('|')
        let attr =arr.[2].Split(',')
        let items=arr.[3].Split('#')
        let arr2D=Array2D.zeroCreate items.Length 2
        for i in 0..items.Length|>(fun m -> m - 1) do
            let subItems=items.[i].Split('>')
            arr2D.[i,0] <- subItems.[0]
            arr2D.[i,1] <- subItems.[1]
        {Section={Name=arr.[0];Node={Name=arr.[1];Attrs=attr}};Items=arr2D}

    let GetServiceConfigFilePath (serviceName:string) (serviceDir:string) =
        let path1= Path.Combine(serviceDir,Path.Combine(serviceName,"Web.config"))
        match File.Exists path1 with
        |true -> path1
        |false -> Path.Combine(serviceDir,"Web.config")

    let GetConfigFileInfo (fileName:string) =
        use file = new FileStream(fileName,FileMode.Open,FileAccess.Read)
        use reader=new StreamReader(file)
        let rec readerByLine (init:string list) =
            let line=reader.ReadLine()
            if reader.EndOfStream then line::init|>List.toArray
            else readerByLine (line::init)
        let result=readerByLine []
        let serviceName=result.[(result.Length) - 1]
        let saveItems= seq{for i in 0..(result.Length - 2) -> result.[i]}|>Seq.map GetConfigContentFromStr
        (serviceName,saveItems)
    

    let GetSavedConfigFileInfo fileName =
        let target = GetConfigFileInfo fileName
        snd target|>Seq.map (fun m -> 
            let items =m.Items
            let attrs=m.Section.Node.Attrs
            let seqDict = seq{
                for i in 0..Array2D.length1(items)|>(fun n -> n - 1) do
                    let dict=new Dictionary<_,_>()
                    dict.Add(attrs.[0],items.[i,0]) 
                    dict.Add(attrs.[1],items.[i,1]) 
                    yield dict
            }
            (m.Section.Name,m.Section.Node.Name,m.Section.Node.Attrs,seqDict)
        )|>(fun m -> ((fst target),m))


    let private GetSavedXMLSectionInfo (m:XElement) =
        let sectionName= m.Element(UtilModule.xn "Name").Value
        let nodeName=m.Element(UtilModule.xn "NodeName").Value
        let attrs=m.Element(UtilModule.xn "Attr").Value.Split([|'|'|])
        let contents= m.Element(UtilModule.xn "Content").Elements() |>Seq.map (fun mm ->
            let dict= new Dictionary<string,string>()
            dict.Add(attrs.[0],string mm.Name)
            dict.Add(attrs.[1],mm.Value)
            dict
            )
        {Section={Name=sectionName;Node={Name=nodeName;Attrs=attrs}};Content=contents|>Array.ofSeq}

    
    let GetSavedConfigFileInfoFromXML (filePath:string) =
        try
            let root=XElement.Load(filePath)
            match root.Elements(UtilModule.xn "Setting")|>Seq.isEmpty with
            |true -> Seq.empty
            |false ->
                root.Elements(UtilModule.xn "Setting")
                |>Seq.map (fun m -> 
                    let serviceName= m.Element(UtilModule.xn "ServiceName").Value
                    let sectionInfo=m.Element(UtilModule.xn "Sections").Elements()
                                    |>Seq.map (fun mm ->
                                        GetSavedXMLSectionInfo mm
                                    )
                    {ServiceName=serviceName;Items=sectionInfo|>Array.ofSeq}
                    )
        with
        |ex -> raise ex
        


    let GetSavedConfigFileItemFromXML (filePath:string) (serviceName:string) =
        try
            let root=XElement.Load(filePath)
            root.Elements(UtilModule.xn "Setting")
            |>Seq.filter (fun m -> m.Element(UtilModule.xn "ServiceName").Value=serviceName)
            |>Seq.head
            |>(fun m -> 
                    let serviceName= m.Element(UtilModule.xn "ServiceName").Value
                    let sectionInfo=m.Element(UtilModule.xn "Sections").Elements()|>Seq.map (fun mm ->GetSavedXMLSectionInfo mm)
                    {ServiceName=serviceName;Items=sectionInfo|>Array.ofSeq}
                  )
        with
        |x -> raise x






    let private SaveContentToOriginConfigFileHelper (fileName:string) (saveItems:SaveItem seq)=
        let root=XElement.Load(fileName)
        saveItems|>Seq.iter (fun m -> 
                match Seq.isEmpty (root.Descendants(UtilModule.xn m.Section.Name)) with
                |true ->save fileName m true
                |false -> 
                    save fileName m false
            )
        true
    

    let SaveContentToFile savedFileName serviceDir=
        try
            let target=GetConfigFileInfo savedFileName
            let fileName=GetServiceConfigFilePath (fst target) serviceDir
            SaveContentToOriginConfigFileHelper fileName (snd target)
        with
        |x -> 
            raise x

    
    let GetSavedServiceItem serviceName (filePath:string) =
        let root=XElement.Load(filePath)
        root.Elements(UtilModule.xn "Setting")
        |>Seq.filter (fun m -> m.Element(UtilModule.xn "ServiceName").Value=serviceName)
        |>Seq.map (fun m ->
            let serviceName= m.Element(UtilModule.xn "ServiceName").Value
            let sectionInfo= m.Element(UtilModule.xn "Sections").Elements()
                             |>Seq.map (fun mm ->
                                        GetSavedXMLSectionInfo mm
                                    )
            {ServiceName=serviceName;Items=sectionInfo|>Array.ofSeq}
            )
        |>Seq.head

    
    let ConvertSeqDictToArray2d (m:Dictionary<string,string> array) =
                let array2D=Array2D.zeroCreate m.Length 2
                m
                |>Array.iteri (fun i mm ->
                    let target = mm.Values.ToArray()
                    array2D.[i,0] <- target.[0]
                    array2D.[i,1] <- target.[1]
                    )
                array2D


    let Apply saveFilePath serviceName dir =
        try
            let configPath=GetServiceConfigFilePath serviceName dir
            let saveXmlServiceItem=GetSavedServiceItem serviceName saveFilePath
            let saveItems= saveXmlServiceItem.Items|>Seq.ofArray|>Seq.map (fun m -> {Section=m.Section;Items=ConvertSeqDictToArray2d (m.Content) })
            SaveContentToOriginConfigFileHelper configPath saveItems
        with
        |x -> raise x
        



    let SaveContentToOriginConfigFile (fileName:string) (saveItems:SaveItem seq) =
        try
        SaveContentToOriginConfigFileHelper fileName saveItems
        with
        |x -> raise x

    
   

    let SaveConfigToTxt (fileName:string) (serviceName:string) (saveItems:SaveItem array) =
       try
            use file = new FileStream(fileName,FileMode.Create,FileAccess.Write)
            use writer= new StreamWriter(file)
            writer.WriteLine(serviceName)
            saveItems|>Array.iter (fun m -> 
                let sb=new StringBuilder()
                sb.Append(sprintf "%s|%s|%s|" (m.Section.Name) (m.Section.Node.Name) (m.Section.Node.Attrs|>(fun x ->x.[0] + "," + x.[1]) ))|>ignore
                for i in 0..Array2D.length1(m.Items)|>(fun n->n - 1) do
                    sb.Append(sprintf "%s>%s" (m.Items.[i,0]) (m.Items.[i,1]))|>ignore
                    sb.Append("#")|>ignore
                writer.WriteLine(sb.ToString()|>(fun x -> x.Substring(0,x.Length - 1)))
            )
            true
        with
        |x -> raise x
   

    let SaveConfigToXml filePath sampleFilePath (serviceItems:WorkItem array) =
        try
            File.Copy(sampleFilePath,filePath,true)
            let root =XElement.Load(filePath)
            serviceItems|>Array.iter (fun m ->
                let serviceElem= new XElement(UtilModule.xn "Setting")
                root.Add(serviceElem)
                serviceElem.SetElementValue((UtilModule.xn "ServiceName"),m.ServiceName)
                let sectionsElem = new XElement(UtilModule.xn "Sections")
                serviceElem.Add(sectionsElem)
                m.SavedInfo|>Array.iter (fun mm ->
                    let sectionElem=new XElement(UtilModule.xn "Section")
                    sectionsElem.Add(sectionElem)
                    sectionElem.SetElementValue((UtilModule.xn "Name"),mm.Section.Name)
                    sectionElem.SetElementValue((UtilModule.xn "NodeName"),mm.Section.Node.Name)
                    sectionElem.SetElementValue((UtilModule.xn "Attr"),mm.Section.Node.Attrs|>String.concat("|"))
                    let contentElem =new XElement(UtilModule.xn "Content")
                    sectionElem.Add(contentElem)
                    for i in 0..Array2D.length1(mm.Items)|>(fun n->n - 1) do
                        contentElem.SetElementValue((UtilModule.xn mm.Items.[i,0]),mm.Items.[i,1])
                    )
                )
            root.Save(filePath)
            true
        with
        |ex -> raise ex


    

    let CheckConfigPathExist (dir:string) (serviceName:string array) =
        serviceName
        |>Array.map (fun m -> 
            let path=GetServiceConfigFilePath m dir
            File.Exists(path)
            )
        |>Array.filter (fun m -> m=false)
        |>Array.isEmpty
        
      


module PublicSettingModule =
    
    let ApplyPublicSettingHelper (publicSettingFileName:string) (targetFileName:string) (section:SectionInfo)=
        let root1=XElement.Load(publicSettingFileName).Descendants(UtilModule.xn (section.Name)).Single()
        let root2=XElement.Load(targetFileName)
        let key=section.Node.Attrs.[0]
        let value=section.Node.Attrs.[1]
        let nodeName=section.Node.Name
        let isRoot2WithXmlns() =
            match Seq.isEmpty (root2.Descendants(UtilModule.xn (section.Name))) with
            |true -> (true,root2.Descendants(StringsModule.xmlns + (section.Name)).Single())
            |false -> (false, root2.Descendants(UtilModule.xn (section.Name)).Single())

        let (isXmlns,target)=isRoot2WithXmlns()
        let createElement()=
            match isXmlns with
            |true -> new XElement(StringsModule.xmlns + nodeName)
            |false -> new XElement(UtilModule.xn nodeName)
        let encryt (ele:XElement) k v =
            match Regex.IsMatch(k,StringsModule.connectionString,RegexOptions.IgnoreCase) with
            |true -> ele.SetAttributeValue((UtilModule.xn value),(Encrypt v))
            |false -> ele.SetAttributeValue((UtilModule.xn value),v)
        root1.Descendants()|> Seq.map (fun m -> ((UtilModule.getAttr m key),(UtilModule.getAttr m value)))
        |>Seq.iter (fun (m,n) ->
            match target.Descendants().SingleOrDefault(fun x -> (UtilModule.getAttr x key).ToLower()=m.ToLower()) with
            |null ->
                let ele=createElement()
                ele.SetAttributeValue((UtilModule.xn key),m)
                encryt ele m n
                target.Add(ele)
            |element ->
                encryt element m n
            )
        root2.Save(targetFileName)
            

    let ApplyPublicSetting (publicSettingFileName:string) (targetFileName:string) (sections:SectionInfo array) =
        try
            sections|>Seq.iter (ApplyPublicSettingHelper publicSettingFileName targetFileName)
            true
        with
        |x -> raise x



    let private ConvertArray2DToDict (items:string[,]) =
        let dict= new Dictionary<_,_>()
        for i in 0..Array2D.length1(items)|>(fun n -> n - 1) do
            if not (dict.ContainsKey(items.[i,0])) then
                dict.Add(items.[i,0],items.[i,1])
        dict


    let private ConvertDictToArray2D (dict:Dictionary<string,string>) =
        let array2D=Array2D.zeroCreate (dict.Keys.Count) 2
        let keys=dict.Keys.ToArray()
        for i in 0..dict.Count|>(fun n -> n - 1) do
            array2D.[i,0] <- keys.[i]
            array2D.[i,1]  <- dict.[keys.[i]]
        array2D

  
    type private SectionResult =
    |None
    |HasValue of SectionInfo * Dictionary<string,string>

    let private ApplyPublicSettingsToXmlHelper (root:XElement) (savedFilePath:string) (psFilePath:string) serviceName =
        let xmlServiceItem= ConfigModule.GetSavedServiceItem serviceName savedFilePath
        let target = xmlServiceItem.Items|>Array.map (fun m ->
                let key = m.Section.Node.Attrs.[0]
                let value=m.Section.Node.Attrs.[1]
                let dict=ConvertArray2DToDict(ConfigModule.ConvertSeqDictToArray2d m.Content)
                let commonRoot=XElement.Load(psFilePath).Descendants(UtilModule.xn (m.Section.Name)).SingleOrDefault()
                match commonRoot with
                |null ->None
                |_ ->
                    commonRoot.Descendants()|> Seq.map (fun mm -> (UtilModule.getAttr mm key),(UtilModule.getAttr mm value))|>
                    Seq.iter (fun (x,y) ->
                        match x with
                        |"" -> ()
                        |_ ->
                            if dict.ContainsKey(x) then dict.[x] <- y
                            else dict.Add(x,y)
                        )
                    HasValue(m.Section,dict)
              )
        target|>Array.iter (fun r ->
                match r with
                |None -> ()
                |HasValue(m,n) ->
                    let serviceElem=root.Elements(UtilModule.xn "Setting")|>Seq.filter (fun x -> x.Element(UtilModule.xn "ServiceName").Value=xmlServiceItem.ServiceName)|>Seq.head
                    let contentElem=serviceElem.Element(UtilModule.xn "Sections").Elements(UtilModule.xn "Section")|>Seq.filter(fun s-> s.Element(UtilModule.xn "Name").Value=m.Name)|>Seq.head|>(fun s ->s.Element(UtilModule.xn "Content"))
                    n.Keys.ToArray()
                    |>Array.iter (fun x ->
                        contentElem.SetElementValue((UtilModule.xn x),n.[x])
                        )
                )


    let ApplyPublicSettingsToSavedXmlFiles (psFilePath:string) (savedFilePath:string) serviceName =
       try
            let root=XElement.Load savedFilePath
            ApplyPublicSettingsToXmlHelper root savedFilePath psFilePath serviceName
            root.Save savedFilePath
            true
       with
       |x -> raise x
                        
        
    let ApplyPublicSettingsToXmlSavedFileAll (psFilePath:string) (savedFilePath:string) =
        try
            let root= XElement.Load(savedFilePath)
            root.Elements(UtilModule.xn "Setting")
            |>Seq.iter (fun m -> 
                let serviceName= m.Element(UtilModule.xn "ServiceName").Value
                ApplyPublicSettingsToXmlHelper root savedFilePath psFilePath serviceName
                )
            root.Save savedFilePath
            true
        with
        |x -> raise x
    
      