using System.Collections.Generic;

[System.Serializable]
public class GameTextData
{
    public List<string> facts;
    public List<string> LinesV1;
    public List<string> LinesV2;
}


[System.Serializable]
public class NPCResponse
{
    public string npcId;
    public string prompt;
    public string response;
}

[System.Serializable]
public class NPCResponseList
{
    public List<NPCResponse> responses = new List<NPCResponse>();
}

