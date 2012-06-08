module StringExtension
type System.String with 
    member this.AsString() =
        match this with
        |x when System.String.IsNullOrEmpty(x) -> None
        |_ ->Some(this)

