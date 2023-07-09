

using SangoPriorityQueue;

Item item1 = new Item("one",10);
Item item2 = new Item("two",7600);
Item item3= new Item("three",130);
Item item4 = new Item("four",660);

SangoPriorityQueue<Item> que = new SangoPriorityQueue<Item>();
que.Enqueue(item1);
que.Enqueue(item2);
que.Enqueue(item3);
que.Enqueue(item4);

//while (que.Count > 0)
//{
//    Item item = que.Dequeue();
//    item.PrintInfo();
//}


Item item = que.RemoveItem(item4);
Console.WriteLine(item);

Console.WriteLine("----------------");

while (que.Count > 0)
{
    Item empitem = que.Dequeue();
    empitem.PrintInfo();
}

Console.ReadKey();

class Item : IComparable<Item>
{
    public string itemName;
    public float priority;

    public int CompareTo(Item other)
    {
        if (priority < other.priority)
        {
            return -1;
        }
        else if (priority > other.priority)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public void PrintInfo()
    {
        Console.WriteLine($"item name:{itemName}   priority:{priority}");
    }

    public Item(string name, float pri)
    {
        itemName = name;
        priority = pri;
    }
}

