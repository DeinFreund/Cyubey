using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System;

public class Account {

    private static Dictionary<string, Account> accounts;

    public static void loadAccounts()
    {
        accounts = new Dictionary<string, Account>();
        Field field = new Field (FileIO.read("accounts.xml"));
        foreach (Field data in field.getFields("account"))
        {
            Account acc = new Account(data);
            accounts.Add(acc.getName(), acc);
        }
    }


    public static void saveAccounts()
    {
        Field field = new Field();
        foreach (Account acc in accounts.Values)
        {
            field.addField("account", acc.serialize());
        }
        FileIO.write("accounts.xml", field.serialize());
    }

    public static Account registerAccount(string username, string password)
    {
        accounts.Add(username, new Account(username));
        accounts[username].setPassword(password);
        return accounts[username];
    }

    public static Account getAccount(string username)
    {
        return accounts.ContainsKey(username) ? accounts[username] : null;
    }

    //-----------------------------

    private string name;
    private byte[] password;
    private byte[] salt;
    private Client client = null;

    public Account(string name)
    {
        this.name = name;
    }

    public Account(Field field)
    {
        name = field.atField("name").getString();
        password = field.atField("password").getValue();
        salt = field.atField("salt").getValue();
    }

    public Field serialize()
    {
        Field field = new Field();
        field.atField("name").setString(name);
        field.atField("password").setValue(password);
        field.atField("salt").setValue(salt);
        return field;
    }

    public bool login(Client client, string password)
    {
        if (!checkPassword(password)) return false;
        this.client = client;
        return true;
    }

    public void logout()
    {
        client = null;
    }
    
    public void setPassword(string newpw)
    {
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        salt = new byte[16];
        rng.GetBytes(salt);
        password = hash(newpw, salt);
    }

    private byte[] hash(string pw, byte[] salt)
    {
        Rfc2898DeriveBytes hashed = new Rfc2898DeriveBytes(pw, salt);
        return hashed.GetBytes(32);
    }

    protected bool checkPassword(string password)
    {
        bool retval = true;
        byte[] hashed = hash(password, salt);
        for (int i = 0; i < hashed.Length; i++)
        {
            if (hashed[i] != this.password[i]) retval = false; 
        }
        return retval;
    }

    public string getName()
    {
        return name;
    }
}
