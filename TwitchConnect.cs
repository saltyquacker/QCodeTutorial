//saltyquacker.games@gmail.com
//Date:    10/14/2022
//Purpose: Manage users who have interacted with 
//         your Twitch Channel.
using System;
using System.IO;
using UnityEngine;
using System.Net.Sockets;
using System.Collections.Generic;

public class TwitchConnect : MonoBehaviour
{
    //Keeping a local list of users prevents users from creating multiple prefabs
    private List<string> activeUsers = new List<string>();

    private TcpClient twitchClient;
    private StreamReader reader;
    private StreamWriter writer;

    [SerializeField] private string username, password, channelName; //Get the password from https://twitchapps.com/tmi

    public GameObject duck;

    void Start()
    {
        //Connect to new 
        Connect();

        //Routinely re-establishes connection as it drops
        //after a certain amount of time, after 5 seconds,
        //and repeat every 5 seconds
        InvokeRepeating("EnsureConnection", 5.0f, 5.0f);
    }

    void Update()
    {
        //Monitor chat in Twitch Channel
        ReadChat();
    }

    void EnsureConnection()
    {
        //Refresh connection
        Connect();
    }

    private void Connect()
    {
        //Connect to Twitch Client
        twitchClient = new TcpClient("irc.chat.twitch.tv", 6667);
        reader = new StreamReader(twitchClient.GetStream());
        writer = new StreamWriter(twitchClient.GetStream());

        writer.WriteLine("PASS " + password);
        writer.WriteLine("NICK " + username);
        writer.WriteLine("USER " + username + " 8 * :" + username);
        writer.WriteLine("JOIN #" + channelName);
        writer.Flush();
        print("Trying to Connect...");
    }

    private void ReadChat()
    {
        if (twitchClient.Available > 0)
        {
            //reads in message
            var message = reader.ReadLine();

            //Chat is returned to Unity as a private message
            if (message.Contains("PRIVMSG"))
            {
                //Get the user and message by splitting it from the string
                var splitPoint = message.IndexOf("!", 1);
                var chatName = message.Substring(0, splitPoint);
                chatName = chatName.Substring(1);
                splitPoint = message.IndexOf(":", 1);
                message = message.Substring(splitPoint + 1);
                message = message.ToLower();

                //Optional print to view chats if debugging
                //print(String.Format("{0}: {1}", chatName, message));

                //Monitor for keyword/command
                if (message == "!quack")
                {
                    //Make new duck
                    if (!activeUsers.Contains(chatName))
                    {
                        //Creates new movable animal object (duck) based
                        //on last user who chatted the keyword
                        GameObject newDuck = Instantiate(duck, new Vector3(-0.05f, -4.59f, 0f), Quaternion.identity);
                        //**Uncomment this to display the player's name on their spawned prefab
                        //AIMovement duckScript = newDuck.GetComponent<AIMovement>();
                        //duckScript.SetName(chatName);
                        activeUsers.Add(chatName);

                        //**Uncomment the code here to integrate with DynamoDB
                        //Grab AWSManager script from the AWSPrefab game object
                        //GameObject aws = GameObject.Find("AWSPrefab");
                        //AWSManager awsScript = aws.GetComponent<AWSManager>();
                        //string dt = DateTime.Now.ToString();

                        //SaveDataTime() to add new date and time to user's
                        //list on your DynamoDB table
                        //awsScript.SaveDateTime(dt, chatName);
                    }
                }
            }

        }
    }
}


