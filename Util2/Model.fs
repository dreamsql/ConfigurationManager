namespace ConfiguationPersistence
open System.Collections.Generic
module ModelModule =
    type NodeInfo={Name:string;Attrs:string array}

    type SectionInfo={Name:string;Node:NodeInfo}

    type ConfigFileInfo={Path:string;RootElement:string;Sections:SectionInfo array}
    type SaveItem={Section:SectionInfo;Items:string[,]}

    type WorkItem={ServiceName:string;SavedInfo:SaveItem array}

    type SavedXMLItem ={Section:SectionInfo;Content:Dictionary<string,string> array}
    type SavedXMLServiceItem={ServiceName:string;Items:SavedXMLItem array}




