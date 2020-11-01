using MongoDB.Driver;
using UnityEngine;

public class Mongo
{
    private const string MONGO_URL = "mongodb+srv://MongoDBUser:8yh0G6zjLfLCYMXC@cluster0.sgs4j.mongodb.net/UserDB?retryWrites=true&w=majority";
    private const string DATABASE_NAME = "UserDB";

    private MongoClient client;
    private IMongoDatabase dataBase;

    private IMongoCollection<Model_Account> accountsCollection;

    public void Init()
    {
        client = new MongoClient(MONGO_URL);
        dataBase = client.GetDatabase(DATABASE_NAME);
        accountsCollection = dataBase.GetCollection<Model_Account>("account");
        Debug.Log("Database has beed initiliazed");
    }

    public void Shutdown()
    {
        client = null;
        accountsCollection = null;
        dataBase = null;
    }

    #region Insert
    public bool InsertAccount(string username, string password, string email)
    {
        //check valid email
        if (!Utility.IsEmail(email))
        {
            Debug.Log(email + " is not a valid");
            return false;
        }

        //check valid username
        if (!Utility.IsUsername(username))
        {
            Debug.Log(username + " is not a valid");
            return false;
        }

        //check if the account already exist
        if (FindAccountByEmail(email) != null)
        {
            Debug.Log(email + " already being used");
            return false;
        }

        Model_Account newAccount = new Model_Account();
        newAccount.Username = username;
        newAccount.ShaPassword = password;
        newAccount.Email = email;

        accountsCollection.InsertOne(newAccount);

        return true;
    }
    public Model_Account LoginAccount(string usernameOrEmail, string password, int cnnId, string token)
    {
        Model_Account accountForLoggining = null;
        
        //Find account
        if(Utility.IsEmail(usernameOrEmail))
        {
            //if looging use email
            accountForLoggining = accountsCollection.Find(u => u.Email.Equals(usernameOrEmail) && u.ShaPassword.Equals(password)).FirstOrDefault();
        }
        else
        {
            //if logging use username
            accountForLoggining = accountsCollection.Find(u => u.Username.Equals(usernameOrEmail) && u.ShaPassword.Equals(password)).FirstOrDefault();
        }

        if(accountForLoggining != null)
        {
            accountForLoggining.ActiveConnection = cnnId;
            accountForLoggining.Token = token;
            accountForLoggining.Status = 1;
            accountForLoggining.LastLogin = System.DateTime.Now;

            accountsCollection.FindOneAndReplace(u => u.Username.Equals(accountForLoggining.Username), accountForLoggining);
        }
        else
        {

        }

        return accountForLoggining;
    }
    #endregion

    #region Fetch
    public Model_Account FindAccountByEmail(string email)
    {
        return accountsCollection.Find(u => u.Email.Equals(email)).FirstOrDefault();
    }
    #endregion

    #region Update
    #endregion

    #region Delete
    #endregion

}