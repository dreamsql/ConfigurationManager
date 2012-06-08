module Configuration.GridHelper
open System.Windows.Controls
open System.Windows
let AddRowDefinition (grid:Grid) count =
    [1..count]|>List.iter (fun i -> grid.RowDefinitions.Add(new RowDefinition()))
let AddColumnDefinition (grid:Grid) count =
    [1..count]|> List.iter (fun i->
        match i with
        |1 -> grid.ColumnDefinitions.Add(new ColumnDefinition(Width=GridLength.Auto))
        |_ -> grid.ColumnDefinitions.Add(new ColumnDefinition())
       )


let MapTypeNameToType typeName =
    match typeName with
    |"string" -> typeof<string>
    |"bool" -> typeof<bool>
    |"enum" -> typeof<System.Enum>
    |"int" -> typeof<int>
    |_ -> typeof<obj>