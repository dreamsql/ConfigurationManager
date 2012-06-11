namespace ConfiguationPersistence
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
        root.Element(UtilModule.xn "Mappings").Elements(UtilModule.xn "Mapping")
        |>Seq.map (fun m -> 
            match m.Elements(UtilModule.xn "Section")|>Seq.isEmpty with
            |true ->(UtilModule.getAttr m "key" ,UtilModule.getAttr m "value",null)
            |false -> 
                let info=m.Elements(UtilModule.xn "Section")|>Seq.map (fun mm ->
                    let sectionName= UtilModule.getAttr mm "name"
                    let node=mm.Descendants().SingleOrDefault()
                    match node with
                    |null ->(sectionName,null,null)
                    |_ ->
                        let nodeName=UtilModule.getAttr node "name"
                        let attrs=UtilModule.getAttr node "attrs"
                        (sectionName,nodeName,attrs)
                    )
                match Seq.isEmpty info with
                |true ->(UtilModule.getAttr m "key" ,UtilModule.getAttr m "value",null)
                |false ->(UtilModule.getAttr m "key" ,UtilModule.getAttr m "value",info)
                
            )