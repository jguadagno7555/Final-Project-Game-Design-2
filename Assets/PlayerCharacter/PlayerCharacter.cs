﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;
//using UnityEngine.SceneManagement;

public class PlayerCharacter : NetworkComponent
{
    public Text PlayerLabel;
    public Text PlayerScore;
    public Text PlayerWood;
    public Text PlayerGold;
    public Text PlayerIron;
    public Slider PlayerHealth;
    public string playerName = "Blank";
    public int playerScore = 0;
    public int playerHealth = 100;
    public int playerWood = 100;
    public int playerIron = 100;
    public int playerGold = 100;
    public Color teamColor = new Color(1,1,1,1);
    public bool isAlive = true;
    private bool clientUnitColorSet = false;
    public Transform disconnectUI;
    

    public Color ParseCV4(string v)
    {
        Color temp = new Color();
        string[] args = v.Trim('(').Trim(')').Split(',');
        temp.r = float.Parse(args[0]); 
        temp.g = float.Parse(args[1]);
        temp.b = float.Parse(args[2]);
        temp.a = float.Parse(args[3]);
        return temp;
    }

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "NAME")
        {
            playerName = value;
            PlayerLabel.text = playerName;
        }
        if(flag == "SCORE")
        {
            
            if(IsServer)
            {
                GameMasterScript tempGMS = FindObjectOfType<GameMasterScript>();
                switch(gameObject.GetComponent<NetworkID>().Owner)
                {
                    case 0:
                        tempGMS.playerScores[0] = playerScore;
                        break;
                    case 1:
                        tempGMS.playerScores[1] = playerScore;
                        break;
                    case 2:
                        tempGMS.playerScores[2] = playerScore;
                        break;
                    case 3:
                        tempGMS.playerScores[3] = playerScore;
                        break;
                    default:
                        break;
                }
            }
            if(IsClient)
            {
                playerScore = int.Parse(value);
                PlayerScore.text = playerScore.ToString();
            }
        }
        if(flag == "HEALTH")
        {
            playerHealth = int.Parse(value);
            if(IsClient)
            {
//                Debug.Log("Player Health Client: " + playerHealth);
            }
            PlayerHealth.value = playerHealth;
            if(IsServer)
            {
                SendUpdate("HEALTH", value);
            }
        }
        if(flag == "TEAMCOLOR")
        {
            teamColor = ParseCV4(value);
            if(IsClient)
            {
                clientUnitColorSet = true;
            }
        }
        if(flag == "WOOD")
        {
            playerWood = int.Parse(value);
            PlayerWood.text = "Wood: " + playerWood.ToString();
            if(IsServer)
            {
                SendUpdate("WOOD", value);
            }
        }
        if(flag == "IRON")
        {
            playerIron = int.Parse(value);
            PlayerIron.text = "Iron: " + playerIron.ToString();
            if(IsServer)
            {
                SendUpdate("IRON", value);
            }
        }
        if(flag == "GOLD")
        {
            playerGold = int.Parse(value);
            PlayerGold.text = "Gold: " + playerGold.ToString();
            if(IsServer)
            {
                SendUpdate("GOLD", value);
            }
        }
    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
      while(IsConnected)
        {
            if(IsServer)
            {
                if(playerHealth <= 0)
                {
                    isAlive = false;
                    NetworkID[] temp = GameObject.FindObjectsOfType<NetworkID>();
                    foreach(NetworkID obj in temp)
                    {
                        if(gameObject.GetComponent<NetworkID>().Owner == obj.Owner)
                        {
                            if(gameObject.GetComponent<PlayerCharacter>()!=null)
                            {
                                GameMasterScript tempGMS = FindObjectOfType<GameMasterScript>();
                                switch(gameObject.GetComponent<NetworkID>().Owner)
                                {
                                    case 0:
                                        tempGMS.playerScores[0] = playerScore;
                                        tempGMS.playerAlive[0] = false;
                                        break;
                                    case 1:
                                        tempGMS.playerScores[1] = playerScore;
                                        tempGMS.playerAlive[1] = false;
                                        break;
                                    case 2:
                                        tempGMS.playerScores[2] = playerScore;
                                        tempGMS.playerAlive[2] = false;
                                        break;
                                    case 3:
                                        tempGMS.playerScores[3] = playerScore;
                                        tempGMS.playerAlive[3] = false;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            MyCore.NetDestroyObject(obj.NetId);
                        }
                    }
                }
                if(IsDirty)
                {
                    SendUpdate("NAME", playerName);
                    SendUpdate("SCORE", playerScore.ToString());
                    SendUpdate("HEALTH", playerHealth.ToString());
                    SendUpdate("TEAMCOLOR", teamColor.r.ToString() + ", " + teamColor.g.ToString() + ", " + teamColor.b.ToString() + ", " + teamColor.a.ToString());
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(.1f);
        }
    }
    

    public void EnableCanvasIG()
    {
        if(IsLocalPlayer)
        {
            this.transform.GetChild(3).gameObject.SetActive(true);
        }
    }

    public void DamageTaken(int d)
    {
        if(IsServer)
        {
            playerHealth -= d;
            SendUpdate("HEALTH", playerHealth.ToString());
        }   
    }

    public void MinedResource(string mine)
    {
        if(IsServer)
        {
            if(mine == "Gold")
            {
                playerGold += 3;
                playerScore += 5;
                SendUpdate("GOLD", playerGold.ToString());
                SendUpdate("SCORE", playerScore.ToString());
            }
            if(mine == "Iron")
            {
                playerIron += 5;
                playerScore += 3;
                SendUpdate("IRON", playerIron.ToString());
                SendUpdate("SCORE", playerScore.ToString());
            }
            if(mine == "Wood")
            {
                playerWood += 10;
                playerScore += 1;
                SendUpdate("WOOD", playerWood.ToString());
                SendUpdate("SCORE", playerScore.ToString());
            }
        }   
    }
    
    public void RemoveResources(int amountWood, int amountIron, int amountGold)
    {
        if(IsServer)
        {
            if(amountGold <= playerGold)
            {
                playerGold -= amountGold;
                SendUpdate("GOLD", playerGold.ToString());
            }
            if(amountIron <= playerIron)
            {
                playerIron -= amountIron;
                SendUpdate("IRON", playerIron.ToString());
            }
            if(amountWood <= playerWood)
            {
                playerWood -= amountWood;
                SendUpdate("WOOD", playerWood.ToString());
            }
        }
    }

    public void AddScore(int amount)
    {
        if(IsServer)
        {
            playerScore += amount;
            SendUpdate("SCORE", playerScore.ToString());
        }
    }

    void Update()
    {
            if(IsClient)
            {
                if(clientUnitColorSet)
                {
//                    Debug.Log("Color Loop");
                    SpriteRenderer temp = gameObject.GetComponent<SpriteRenderer>();
                    if(temp!=null)
                    {
                        temp.color = teamColor;
//                        Debug.Log("Unit Color: " + teamColor);
//                        Debug.Log("Sprite Color: " + temp);
                        clientUnitColorSet = false;
                    }
                }
            }
    }

    
}
