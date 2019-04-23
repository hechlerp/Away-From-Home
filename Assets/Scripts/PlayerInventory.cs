using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    Hashtable inventory;
    // Start is called before the first frame update
    void Start()
    {
        inventory = new Hashtable();
    }
    
    public void addToInventory(string name, int amt) {
        if (inventory.ContainsKey(name)) {
            inventory[name] = (int)inventory[name] + amt;
        } else {
            inventory[name] = amt;
        }
    }

    public bool isInInventory(string name) {
        return inventory.ContainsKey(name) & (int)inventory[name] > 0;
    }

    public void removeFromInventory(string name, string amt) {
        if (amt == "all") {
            inventory.Remove(name);
        } else {
            inventory[name] = (int)inventory[name] - int.Parse(amt);
            if ((int)inventory[name] < 0) {
                inventory[name] = 0;
            }
        }
    }
}
