module Configuration.Cryption
open System.Security.Cryptography;
open System.Text
let key="<RSAKeyValue><Modulus>o09NLKTzG4SHQ3zssCS+tri2eZqd9lQ3vjqr2//4M/I0nB4Bu0gWTjCQGx7bTDSustIYS6oKjLyTUXYRsvCrw5weOy2lecbaMu34i7jG+rk0TNBdpEPwrQV7xoAXZ5EeYiSNkZmtUzHv4LcO5MuXW96aU/fswS/g+M4Mvuj+vSE=</Modulus><Exponent>AQAB</Exponent><P>1fIndCdr/Dorob+TpXlJryjn1fGwUqgq1p4YQIOsLMKmSmzxhaWnABLmoCTnyGZu6VMHIbJ0Y+HhUQsIpgvCdQ==</P><Q>w2kdrPfTC+SbcY7OBkDpLneal1prLxrw0C2mmPhc5RuFtpzEJvSnbkfCQazeeV2sdAQ95QSH8sMkm86Tx5BifQ==</Q><DP>eBdkRBP4zm0Mns99ni3VyYeJkxMGaW9SFIRLkrMWi017sF00uVNByY3SfOQaYuf0q+3aG/Ui1gotwqMR6LrDHQ==</DP><DQ>pD45vyQdsyVWsb/B0ufEFlZZDVXmORV/yrpUCMbX7YmQfciN5eBEyiBuWh0ecQwW4vyduVxxl84Fex/KfjYRUQ==</DQ><InverseQ>oxYp1ZDr9vO0VVpHZk00O9iWQXSxzppx+jF9ImdapCeVg/JGxBkGaIA6MirHelYBnYuiDe7xtt77sX2uei7NIg==</InverseQ><D>SBhGNaNMP6WuITkRNGHEX94DkIOVoJ1lTnGQVTsXU7dlSlZk5UzZrAL8WzywC2Bmj0L4vs5+gcruLlQ1VA2zhZ66euzkhSKWkvKhlJbWQZTKFdrObtVqUHbAt1/xzzHWIxBiscBjWQMO6OsWZFjsDdoCxKHv3ssYEA0jCeH2kME=</D></RSAKeyValue>"
let Encrypt (input:string) =
    try
        input
//        let rsaService= new RSACryptoServiceProvider()
//        rsaService.FromXmlString key
//        let byteConverter= new ASCIIEncoding();
//        let bytes=byteConverter.GetBytes(input)
//        rsaService.Encrypt(bytes,false) |>System.Convert.ToBase64String
    with
    |_->input
        


let Decrypt (input:string) =
    try
        input
//        let rsaService= new RSACryptoServiceProvider()
//        rsaService.FromXmlString key
//        let byteConverter= new ASCIIEncoding();
//        let base64Bytes=System.Convert.FromBase64String(input)
//        rsaService.Decrypt(base64Bytes,false)|>byteConverter.GetString
    with
    |_ -> input