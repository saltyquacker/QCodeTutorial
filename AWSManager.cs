//saltyquacker.games@gmail.com
//Date:    10/14/2022
//Purpose: Create a database connection between
//         Unity and AWSDynamoDB for reading, 
//         writing, and deleting data.

using Amazon;
using UnityEngine;
using Amazon.Runtime;
using Amazon.DynamoDBv2;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

//##TableName## replaced with DynamoDB table name

public class AWSManager : MonoBehaviour
{
    private AmazonDynamoDBClient client;
    private DynamoDBContext context;

    //Example Database object for user and list of dates
    [DynamoDBTable("##TableName##")]
    public class DatabaseObject
    {
        [DynamoDBHashKey]
        public string user { get; set; }
        [DynamoDBProperty]
        public List<string> dates { get; set; }
    }

    void Start()
    {
        AmazonDynamoDBConfig clientConfig = new AmazonDynamoDBConfig();

        //This can be grabbed from Amazon Cognito's Federated Identities Pool
        CognitoAWSCredentials credentials = new CognitoAWSCredentials(
            "us-east-1:########-####-####-####-############", // Identity pool ID
            RegionEndpoint.USEast1                            // Region
        );

        client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
        context = new DynamoDBContext(client);
        DescribeTable();
    }

    //Create request
    public void DescribeTable()
    {
        var request = new DescribeTableRequest
        {
            TableName = @"##TableName##"
        };

    }

    //Example property pulls current data in list (if any), adds new date, 
    //and replaces new list of dates into DynamoDB
    public void SaveDateTime(string date, string username)
    {
        List<string> thisNewDate = new List<string> { date };
        DatabaseObject objectRetrieved = null;

        //Try in case the user does not exist
        try
        {
            //Load current user's information if it exists
            var user = context.LoadAsync<DatabaseObject>(username);
            
            //Store results locally of this user from load
            userRetrieved = new DatabaseObject
            {
                user = user.Result.user,
                dates = user.Result.dates
            };

            //Add new list item to stored local list
            userRetrieved.dates.Add(date);

            //Send the updated object that contains the new date in the list
            context.SaveAsync<DatabaseObject>(userRetrieved);
        }
        catch //This will catch if this is a new user
        {
            //Create new database object to hold the user
            DatabaseObject newUser = new DatabaseObject
            {
                user = username,
                dates = thisNewDate
            };
            //Send the new object that contains the user and their first date entry
            context.SaveAsync<DatabaseObject>(newUser);
        }
    }
}
