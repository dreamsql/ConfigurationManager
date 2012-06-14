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
        do xmlNamespaceManager.AddNamespace("x",StringsModule.xmlNamespace)
        sectionInfo|>
        Seq.map (fun (sectionName,nodeName,attrs) ->
            let sectionParts=sectionName.Split('/')|>String.concat "/x:"
            let xpath= ref ""
            xpath:= sprintf "%s/%s" sectionName nodeName
            let mutable nodes=root.XPathEvaluate(!xpath,xmlNamespaceManager) :?>seq<obj>
            match nodes|>Seq.isEmpty with
            |false -> ()
            |true ->
                xpath:=sprintf "/x:%s/x:%s" sectionParts nodeName
                nodes <- root.XPathEvaluate(!xpath,xmlNamespaceManager) :?>seq<obj>
            let contents= nodes|>Seq.map (fun m ->
                let elem= m :?> XElement
                attrs|>Array.map (fun a ->
                    let attr=UtilModule.getAttr elem a
                    match attr with
                    |"" ->
                        let child=elem.Descendants().FirstOrDefault()
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

        

