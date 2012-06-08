[<System.Runtime.CompilerServices.Extension>]
module Extension.DateTimeExtension
[<System.Runtime.CompilerServices.Extension>]
let ToStartDay(s:System.DateTime) = new System.DateTime(s.Year,s.Month,s.Day,0,0,0)
[<System.Runtime.CompilerServices.Extension>]
let ToEndDay(s:System.DateTime) = new System.DateTime(s.Year,s.Month,s.Day,23,59,59)

