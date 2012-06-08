module Util.ErrorService
open System.Collections.Generic
let FormatError count msg =
    sprintf "Error%d  %s" count msg


let GetKeys keyAttr (source:Dictionary<string,string> array) (keys:string []) =
    keys|>Seq.filter (fun m ->
         let n =source|>Array.filter (fun x -> x.[keyAttr] = m)|>Array.length
         n > 0
        )
