using System;

[Serializable]
public class ControlData {
    public string gameType;
    public int allowedNumberOfPlayers;
    public string location;
    public int dinorunNumberOfPlayerPicked; // Optional, only used for DinoRun
}
