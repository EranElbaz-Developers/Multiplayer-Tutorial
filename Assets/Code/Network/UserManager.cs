using System.Collections.Generic;

public class UserManager
{

    public Dictionary<int, PlayerDataServer> users;

    public UserManager()
    {
        users = new Dictionary<int, PlayerDataServer>();
    }
}