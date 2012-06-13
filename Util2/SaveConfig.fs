namespace ConfiguationPersistence
open System.IO
open System.Xml
open System.Xml.XPath
open System.Xml.Linq
open System.Linq

module SaveConfigModule =
    
    let SetService (root:XElement) (serviceName:string) (m:seq<string*string*string[]*seq<System.Collections.Generic.Dictionary<string,string>>>) =
         let serviceElem=new XElement(UtilModule.xn "Setting")
         root.Add(serviceElem)
         serviceElem.SetElementValue((UtilModule.xn "ServiceName"),serviceName)
         let sectionsElem=new XElement(UtilModule.xn "Sections")
         serviceElem.Add(sectionsElem)
         m|>
         Seq.iter (fun (sectionName,nodeName,attrs,content) -> 
                    let sectionElem=new XElement(UtilModule.xn "Section")
                    sectionsElem.Add(sectionElem)
                    sectionElem.SetElementValue((UtilModule.xn "Name"),sectionName)
                    sectionElem.SetElementValue((UtilModule.xn "NodeName"),nodeName)
                    sectionElem.SetElementValue((UtilModule.xn "Attr"),attrs|>String.concat("|"))
                    let contentsElem=new XElement(UtilModule.xn "Contents")
                    sectionElem.Add(contentsElem)
                    content|>
                    Seq.iter (fun mm ->
                        let contentElem=new XElement(UtilModule.xn "Content")
                        contentsElem.Add(contentElem)
                        attrs|>
                        Array.iter (fun attr ->
                            contentElem.SetAttributeValue((UtilModule.xn attr),mm.[attr])
                            )
                        )
                    )

    let private xn name =XName.op_Implicit name
    let Save filePath sampleFilePath (saveInfo:seq<string*seq<string*string*string[]*seq<System.Collections.Generic.Dictionary<string,string>>>>) =
        try
            File.Copy(sampleFilePath,filePath,true)
            let root =XElement.Load(filePath)
            saveInfo|>
            Seq.iter (fun (serviceName,m) ->
               SetService root serviceName m
                )
            root.Save(filePath)
            true   
        with
        |x -> raise x

    
    let private GetSettingInfo (m:XElement) =
        let serviceName= m.Element(xn "ServiceName").Value
        let sectionInfo =m.Element(xn "Sections").Elements(xn "Section")|>Seq.map (fun mm ->
             let sectionname=mm.Element(xn "Name").Value
             let nodeName=mm.Element(xn "NodeName").Value
             let attrs=mm.Element(xn "Attr").Value.Split('|')
             let contents=mm.Element(xn "Contents").Elements(xn "Content")|>Seq.map (fun mmm ->
                                attrs|>Array.map (fun x ->
                                    (x,(UtilModule.getAttr mmm x))
                                    )
                                |>Map.ofArray|>UtilModule.ConvertMapToDictinary
                               )
             (sectionname,nodeName,attrs,contents)
              )
        (serviceName,sectionInfo)

    let Get (path:string) =
        try
            let root=XElement.Load(path)
            root.Elements(xn "Setting")
            |>Seq.map (fun m ->
                GetSettingInfo m
                )
        with
        |x -> raise x

    
    let GetByServiceName (path:string) (serviceName:string) =
        try
            let root = XElement.Load(path)
            root.Elements(xn "Setting").Single(fun m-> m.Element(xn "ServiceName").Value=serviceName)
            |>GetSettingInfo
            
        with
        |x -> raise x


