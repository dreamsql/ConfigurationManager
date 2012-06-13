namespace ConfiguationPersistence
open System.IO
open System.Xml
open System.Xml.XPath
open System.Xml.Linq
open System.Linq

module ApplyPublicSettingModule =
    
    let private ApplyHelper (root:XElement) (publicSettingInfo:seq<string*string*string[]*seq<System.Collections.Generic.Dictionary<string,string>>>) (serviceName:string) =
       let targetElem=root.Elements(UtilModule.xn "Setting").SingleOrDefault(fun m ->m.Element(UtilModule.xn "ServiceName").Value=serviceName)
       match targetElem with
       |null -> ()
       |_ ->
           publicSettingInfo|>
           Seq.iter (fun (sectionName,nodeName,attrs,contents) ->
                let secElem=targetElem.Element(UtilModule.xn "Sections").Elements(UtilModule.xn "Section").SingleOrDefault(fun m->m.Element(UtilModule.xn "Name").Value=sectionName)
                match secElem with
                |null -> ()
                |_ ->
                    contents|>
                    Seq.iter (fun content ->
                        let contentsElem=secElem.Element(UtilModule.xn "Contents")
                        let contentElem=contentsElem.Elements(UtilModule.xn "Content").SingleOrDefault(fun m -> m.Attribute(UtilModule.xn (attrs.[0])).Value=content.[attrs.[0]])
                        match contentElem with
                        |null ->
                            let targetElem=new XElement(UtilModule.xn "Content")
                            contentsElem.Add(targetElem)
                            attrs|>
                            Array.iter (fun attr ->
                                targetElem.SetAttributeValue((UtilModule.xn attr),content.[attr])
                                )
                        |_ ->()
                        )
                        
                    )



    let ApplyAll (publicSettingInfo:seq<string*string*string[]*seq<System.Collections.Generic.Dictionary<string,string>>>) (savedFilePath:string) =
        try
            let root=XElement.Load(savedFilePath)
            let serviceSet=root.XPathEvaluate("Setting/ServiceName") :?> seq<obj>|>Seq.map (fun m ->
                let elem= m :?> XElement
                elem.Value
                )
            serviceSet|>
            Seq.iter (ApplyHelper root publicSettingInfo)
            root.Save(savedFilePath)
            true
        with
        |x -> raise x


    let Apply (publicSettingInfo:seq<string*string*string[]*seq<System.Collections.Generic.Dictionary<string,string>>>) (savedFilePath:string) (serviceName:string) =
        try
            let root=XElement.Load(savedFilePath)
            ApplyHelper root publicSettingInfo serviceName
            root.Save(savedFilePath)
            true
        with
        |x -> raise x


    let ApplyToOriginConfig (publicSettingInfo:seq<string*string*string[]*seq<System.Collections.Generic.Dictionary<string,string>>>) (path:string) =
        try
            let root=XElement.Load(path)
            let xmlTable= root.CreateReader().NameTable
            let xmlNamespaceManager=new XmlNamespaceManager(xmlTable)
            do xmlNamespaceManager.AddNamespace("x",StringsModule.xmlNamespace)
            publicSettingInfo|>
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
                let elems = nodes|>Seq.map (fun m -> m :?> XElement)
                let existKeys = elems|>Seq.map (fun m -> UtilModule.getAttr m (attrs.[0]))
                
                let parentElem= elems.First().Parent
                content|>
                Seq.iter (fun m -> 
                    let target = existKeys.Any(fun x -> x = m.[attrs.[0]])
                    match target with
                    |false ->
                        let targetElem=new XElement(UtilModule.xn nodeName)
                        parentElem.Add(targetElem)
                        attrs|>
                        Array.iter (fun attr ->
                             targetElem.SetAttributeValue((UtilModule.xn attr),m.[attr])
                              )
                    |true -> ()
                )
                )
            root.Save(path)
            true
        with
        |x -> raise x