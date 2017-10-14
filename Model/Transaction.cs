namespace blockchain_parser.Model
{
     public struct Transaction {
            public string hash {get; set;} 
            public string from {get; set;} 
            public string to {get; set;} 
            public string input {get; set;} 
            public string value {get; set;}
    }
}