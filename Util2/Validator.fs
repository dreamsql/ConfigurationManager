
#light
module VilidationLibrary.Validator
open System.Text.RegularExpressions
let private IsMatch pattern input =
    let rex= new Regex(pattern)
    rex.IsMatch(input)

let IsPhone input=
    let rex=new Regex(@"\d{7}\d*$")
    rex.IsMatch(input)

let IsFax input =
    let rex=new Regex(@"^[+]{0,1}(\d){1,5}?(\d){1,4}?(\d){1,10}([-]\d{1,4})?$")
    rex.IsMatch(input)

let IsEmail input =
    let rex= new Regex(@"^(\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*;?)*$")
    rex.IsMatch(input)

let IsSingleEmail input =
    let pattern ="^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"
    IsMatch pattern input

let IsInternationAccessCode input =
    let pattern = "^\d{1,5}$"
    IsMatch pattern input

let IsCountryCode input =
    let pattern ="^\d{1,6}$"
    IsMatch pattern input


let IsAreaCode input =
    let pattern="^\d{2,6}$"
    IsMatch pattern input

let IsDomainName input =
    let pattern="^([\w-]+\.)+([\w-]+)$"
    IsMatch pattern input

let IsPort input =
    let pattern="^[1-9]\d{0,4}$"
    IsMatch pattern input