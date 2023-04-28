public class Node
{
    public int x;
    public int y;

    public int gCost;
    public int hCost;
    public int fCost;

    public Node cameFromNode;

    public bool isWalkable;

    public Node(int x, int y, bool isWalkable)
    {
        this.x = x; //like row and col
        this.y = y;
        hCost = 0;
        this.isWalkable = isWalkable; //prvents wall walkingb
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }
}