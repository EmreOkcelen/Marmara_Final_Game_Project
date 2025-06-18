using System.Collections.Generic;

[System.Serializable]
public class GameTextData
{
    public List<string> facts;
    public List<string> ilkSahne;
    public List<string> Dus;
    public List<string> EvdenAyrýlma;
    public List<string> MetroAyrýlma;
    public List<string> SonSahne;
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

