namespace ConfiguationPersistence
open ConfiguationPersistence.ModelModule
open System.IO
open System.Xml
open System.Xml.Linq
open System.Linq
open System.Collections.Generic
open System.Text.RegularExpressions
open System.Text

module ConfigurationHelperModule =
    let GetMapInfo (path:string) =
        let root=XElement.Load(path)
        root.Element("Mappings").Descendants()
        |>Seq.map (fun m -> 
            match m.Descendants()|>Seq.isEmpty with
            |true ->(UtilModule.getAttr(m,"key"),UtilModule.getAttr(m,"value"),(null,null,null))
            |false -> 
                let info=m.Descendants()|>Seq.map (fun mm ->
                    let sectionName= UtilModule.getAttr mm "name"
                    let node=mm.Descendants().Single()
                    let nodeName=UtilModule.getAttr node "name"
                    let attrs=UtilModule.getAttr node "attrs"
                    (sectionName,node,nodeName,attrs)
                    )
            )