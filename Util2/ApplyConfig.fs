namespace ConfiguationPersistence
open System.IO
open System.Xml
open System.Xml.XPath
open System.Xml.Linq
open System.Linq

module ApplyConfigModule =
    
    let Apply (path:string) (sectionInfo:seq<string*string*string[]*seq<System.Collections.Generic.Dictionary<string,string>>>) =
        try
            let root=XElement.Load(path)
            let xmlTable= root.CreateReader().NameTable
            let xmlNamespaceManager=new XmlNamespaceManager(xmlTable)
            do xmlNamespaceManager.AddNamespace("x",StringsModule.xmlNamespace)
            sectionInfo|>
            Seq.iter (fun (sectionName,nodeName,attrs,content) ->
                let sectionParts=sectionName.Split('/')|>String.concat "/x:"
                let xpath= ref ""
                xpath:= sprintf "%s/%s" sectionName nodeName
                let mutable nodes=root.XPathEvaluate(!xpath,xmlNamespaceManager) :?>seq<obj>
                match nodes|>Seq.isEmpty with
                |false -> ()
                |true -> 
                    xpath:=sprintf "/x:%s/x:%s" sectionParts nodeName
                    nodes <- root.XPathEvaluate(!xpath,xmlNamespaceManager) :?>seq<obj>
                nodes|>
                Seq.iter (fun m ->
                   let elem =m :?>XElement
                   let target = content.SingleOrDefault(fun x ->x.[attrs.[0]]=(UtilModule.getAttr elem (attrs.[0])))
                   match target with
                   |null ->()
                   |_ ->
                        attrs|>Array.iteri (fun i x ->
                            match i with 
                            |0 ->()
                            |_ ->
                                match x with
                                |"value" ->
                                    match elem.Descendants()|>Seq.isEmpty with
                                    |true -> elem.SetAttributeValue((UtilModule.xn x),target.[x])
                                    |false ->
                                        let child=elem.Descendants().Single()
                                        child.SetValue(target.[x])
                                |_ ->
                                    let attVal=UtilModule.getAttr elem x
                                    match attVal with
                                    |"" ->
                                        let child=elem.Descendants().First()
                                        child.SetAttributeValue((UtilModule.xn x),target.[x])
                                    |_ ->
                                        elem.SetAttributeValue((UtilModule.xn x),target.[x])
                        )
                    )
                )
            root.Save(path)
            true
        with
        |x -> raise x



    let ApplyToFile (saveFilePath:string) (serviceName:string) (sectionInfo:seq<string*string*string[]*seq<System.Collections.Generic.Dictionary<string,string>>>) =
        try
            let root=XElement.Load(saveFilePath)
            let target=root.Elements(UtilModule.xn "Setting").Single(fun m -> m.Element(UtilModule.xn "ServiceName").Value=serviceName)
            target.Remove()
            root.Save(saveFilePath)
            let rootNew=XElement.Load(saveFilePath)
            SaveConfigModule.SetService rootNew serviceName sectionInfo
            rootNew.Save(saveFilePath)
            true
        with
        |x -> raise x