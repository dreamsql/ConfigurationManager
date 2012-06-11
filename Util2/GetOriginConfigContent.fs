namespace ConfiguationPersistence
open System.IO
open System.Xml
open System.Xml.XPath
open System.Xml.Linq
open System.Linq
module FetchOriginContentModule =
    let Get (path:string) (sectionInfo:seq<string*string*string[]>) =
        let root=XElement.Load(path)
        let xmlTable= root.CreateReader().NameTable
        let xmlNamespaceManager=new XmlNamespaceManager(xmlTable)
        sectionInfo|>
        Seq.map (fun (sectionName,nodeName,attrs) ->
            let xpath=sprintf "%s/%s" sectionName nodeName
            let nodes=root.XPathEvaluate(xpath,xmlNamespaceManager) :?>seq<obj>
            let contents= nodes|>Seq.map (fun m ->
                let elem= m :?> XElement
                attrs|>Array.map (fun a ->
                    let attr=UtilModule.getAttr elem a
                    match attr with
                    |"" ->
                        let child=elem.Descendants().SingleOrDefault()
                        match child with
                        |null -> (a,"")
                        |_ ->
                            let childAttr=UtilModule.getAttr child a
                            match childAttr with
                            |"" -> (a,child.Value)
                            |_ ->(a,childAttr)
                    |_ ->(a,attr)
                    )|>Map.ofArray|>UtilModule.ConvertMapToDictinary
                )
            (sectionName,nodeName,attrs,contents)
            )

        

