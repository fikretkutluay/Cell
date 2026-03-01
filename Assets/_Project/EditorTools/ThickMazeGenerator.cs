using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor; // Verileri diske kaydetmek i�in gerekli
#endif

public class ThickMazeGenerator : MonoBehaviour
{
    [Header("Tilemap Referans�")]
    public Tilemap mazeTilemap;

    [Header("K��e Tile'lar�")]
    public TileBase koseSolUst;
    public TileBase koseSagUst;
    public TileBase koseSolAlt;
    public TileBase koseSagAlt;

    [Header("Kenar ve �� Tile'lar")]
    public TileBase ortaTileAlt;
    public TileBase ortaTileUst;
    public TileBase ortaTileSol;
    public TileBase ortaTileSag;
    public TileBase tamTile;

    [Header("Ba�lang�� ve Biti� Zemin/Portal Tile'lar�")]
    public TileBase startTile;
    public TileBase endTile;

    [Header("Harita Boyut ve Kal�nl�k Ayarlar�")]
    public int width = 21;
    public int height = 21;
    public int wallThickness = 1;
    public int pathThickness = 4;

    [Header("Yapay Zeka (Graph) Otomasyonu")]
    public GraphData graphData; // Kendi yazd���m�z A* veri dosyas�

    private bool[,] physicalMaze;
    private int physWidth;
    private int physHeight;

    [ContextMenu("Haritay� Olu�tur (Tam Otomasyon)")]
    public void GenerateThickMaze()
    {
        mazeTilemap.ClearAllTiles();

        if (width % 2 == 0) width++;
        if (height % 2 == 0) height++;

        int[,] logicalMaze = new int[width, height];

        // 1. A�AMA: Mant�ksal Haritay� Duvarla Doldur
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                logicalMaze[x, y] = 1;

        // 2. A�AMA: DFS Algoritmas� ile �nce Labirenti Kaz�
        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        Vector2Int startPoint = new Vector2Int(1, height - 2);
        logicalMaze[startPoint.x, startPoint.y] = 0;
        stack.Push(startPoint);

        Vector2Int[] directions = {
            new Vector2Int(0, 2), new Vector2Int(0, -2),
            new Vector2Int(2, 0), new Vector2Int(-2, 0)
        };

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            List<Vector2Int> unvisitedNeighbors = new List<Vector2Int>();

            foreach (var dir in directions)
            {
                Vector2Int neighbor = current + dir;
                if (neighbor.x > 0 && neighbor.x < width - 1 && neighbor.y > 0 && neighbor.y < height - 1)
                {
                    if (logicalMaze[neighbor.x, neighbor.y] == 1)
                        unvisitedNeighbors.Add(dir);
                }
            }

            if (unvisitedNeighbors.Count > 0)
            {
                Vector2Int chosenDir = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
                Vector2Int neighbor = current + chosenDir;
                Vector2Int wallBetween = current + new Vector2Int(chosenDir.x / 2, chosenDir.y / 2);

                logicalMaze[neighbor.x, neighbor.y] = 0;
                logicalMaze[wallBetween.x, wallBetween.y] = 0;

                stack.Push(neighbor);
            }
            else
            {
                stack.Pop();
            }
        }

        Vector2Int endPoint = new Vector2Int(width - 2, 1);
        logicalMaze[endPoint.x, endPoint.y] = 0;

        // 3. A�AMA: Ba��ms�z Kal�nl�klar� Uygulayarak Fiziksel Haritay� ��kar
        physWidth = (width / 2) * pathThickness + ((width + 1) / 2) * wallThickness;
        physHeight = (height / 2) * pathThickness + ((height + 1) / 2) * wallThickness;
        physicalMaze = new bool[physWidth, physHeight];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (logicalMaze[x, y] == 1)
                {
                    int startX = (x / 2) * pathThickness + ((x + 1) / 2) * wallThickness;
                    int startY = (y / 2) * pathThickness + ((y + 1) / 2) * wallThickness;

                    int sizeX = (x % 2 == 0) ? wallThickness : pathThickness;
                    int sizeY = (y % 2 == 0) ? wallThickness : pathThickness;

                    for (int px = 0; px < sizeX; px++)
                    {
                        for (int py = 0; py < sizeY; py++)
                        {
                            physicalMaze[startX + px, startY + py] = true;
                        }
                    }
                }
            }
        }

        // 4. A�AMA: AUTO-TILING ��Z�M�
        for (int px = 0; px < physWidth; px++)
        {
            for (int py = 0; py < physHeight; py++)
            {
                if (physicalMaze[px, py])
                {
                    bool u = IsWall(px, py + 1);
                    bool d = IsWall(px, py - 1);
                    bool l = IsWall(px - 1, py);
                    bool r = IsWall(px + 1, py);

                    TileBase tileToPlace = tamTile;

                    if (!u && d && !l && r) tileToPlace = koseSolUst;
                    else if (!u && d && l && !r) tileToPlace = koseSagUst;
                    else if (u && !d && !l && r) tileToPlace = koseSolAlt;
                    else if (u && !d && l && !r) tileToPlace = koseSagAlt;
                    else if (u && !d && l && r) tileToPlace = ortaTileAlt;
                    else if (!u && d && l && r) tileToPlace = ortaTileUst;
                    else if (u && d && !l && r) tileToPlace = ortaTileSol;
                    else if (u && d && l && !r) tileToPlace = ortaTileSag;

                    int finalX = px - (physWidth / 2);
                    int finalY = py - (physHeight / 2);
                    mazeTilemap.SetTile(new Vector3Int(finalX, finalY, 0), tileToPlace);
                }
            }
        }

        // 5. A�AMA: Ba�lang��, Biti� ve Karakter I��nlama
        int startPhysX = (startPoint.x / 2) * pathThickness + ((startPoint.x + 1) / 2) * wallThickness;
        int startPhysY = (startPoint.y / 2) * pathThickness + ((startPoint.y + 1) / 2) * wallThickness;
        int finalStartPx = startPhysX + (pathThickness / 2) - (physWidth / 2);
        int finalStartPy = startPhysY + (pathThickness / 2) - (physHeight / 2);

        mazeTilemap.SetTile(new Vector3Int(finalStartPx, finalStartPy, 0), startTile);

        int endPhysX = (endPoint.x / 2) * pathThickness + ((endPoint.x + 1) / 2) * wallThickness;
        int endPhysY = (endPoint.y / 2) * pathThickness + ((endPoint.y + 1) / 2) * wallThickness;
        int finalEndPx = endPhysX + (pathThickness / 2) - (physWidth / 2);
        int finalEndPy = endPhysY + (pathThickness / 2) - (physHeight / 2);

        mazeTilemap.SetTile(new Vector3Int(finalEndPx, finalEndPy, 0), endTile);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            Vector3 worldPos = mazeTilemap.GetCellCenterWorld(new Vector3Int(finalStartPx, finalStartPy, 0));
            playerObj.transform.position = new Vector3(worldPos.x, worldPos.y, playerObj.transform.position.z);
        }

        // 6. A�AMA: KEND� GRAPH S�STEM�M�Z� OTOMAT�K DOLDURMA
        if (graphData != null)
        {
            graphData.nodes.Clear();
            GraphNode[,] nodeGrid = new GraphNode[width, height];

            // �nce t�m node'lar� olu�tur
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (logicalMaze[x, y] == 0)
                    {
                        int nodeStartPhysX = (x / 2) * pathThickness + ((x + 1) / 2) * wallThickness;
                        int nodeStartPhysY = (y / 2) * pathThickness + ((y + 1) / 2) * wallThickness;

                        int centerPx = nodeStartPhysX + (pathThickness / 2) - (physWidth / 2);
                        int centerPy = nodeStartPhysY + (pathThickness / 2) - (physHeight / 2);

                        Vector3 worldPos = mazeTilemap.GetCellCenterWorld(new Vector3Int(centerPx, centerPy, 0));

                        GraphNode newNode = new GraphNode();
                        newNode.position = new Vector2(worldPos.x, worldPos.y);

                        nodeGrid[x, y] = newNode;
                        graphData.nodes.Add(newNode);
                    }
                }
            }

            // Sonra index'leri kullanarak ba�lant�lar� olu�tur
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    GraphNode current = nodeGrid[x, y];
                    if (current != null)
                    {
                        // Kom�ular� kontrol et ve index'lerini ekle
                        if (y + 1 < height && nodeGrid[x, y + 1] != null)
                        {
                            int neighborIndex = graphData.nodes.IndexOf(nodeGrid[x, y + 1]);
                            if (!current.neighborIndices.Contains(neighborIndex))
                                current.neighborIndices.Add(neighborIndex);
                        }

                        if (y - 1 >= 0 && nodeGrid[x, y - 1] != null)
                        {
                            int neighborIndex = graphData.nodes.IndexOf(nodeGrid[x, y - 1]);
                            if (!current.neighborIndices.Contains(neighborIndex))
                                current.neighborIndices.Add(neighborIndex);
                        }

                        if (x + 1 < width && nodeGrid[x + 1, y] != null)
                        {
                            int neighborIndex = graphData.nodes.IndexOf(nodeGrid[x + 1, y]);
                            if (!current.neighborIndices.Contains(neighborIndex))
                                current.neighborIndices.Add(neighborIndex);
                        }

                        if (x - 1 >= 0 && nodeGrid[x - 1, y] != null)
                        {
                            int neighborIndex = graphData.nodes.IndexOf(nodeGrid[x - 1, y]);
                            if (!current.neighborIndices.Contains(neighborIndex))
                                current.neighborIndices.Add(neighborIndex);
                        }
                    }
                }
            }

            // Runtime neighbor listelerini olu�tur
            graphData.RebuildNeighborReferences();
        }

        Debug.Log($"Harita �retildi! Duvar: {wallThickness}, Yol: {pathThickness}. Graph Nodelar� ba�land�!");

        // KR�T�K EKLENT�: Verilerin Play'e bas�nca silinmemesi i�in diske zorla kaydetme (Serialization)
#if UNITY_EDITOR
        if (graphData != null)
        {
            EditorUtility.SetDirty(graphData);
            AssetDatabase.SaveAssets();
            Debug.Log($"GraphData saved with {graphData.nodes.Count} nodes!");
        }
#endif
    }

    private bool IsWall(int x, int y)
    {
        if (x < 0 || x >= physWidth || y < 0 || y >= physHeight) return true;
        return physicalMaze[x, y];
    }

    // EKLENEN KISIM: Nodelar� ve aralar�ndaki yollar� sahnede g�rsel olarak �izmemizi sa�lar
    private void OnDrawGizmos()
    {
        if (graphData != null && graphData.nodes != null)
        {
            foreach (var node in graphData.nodes)
            {
                // D���mleri cyan (a��k mavi) renkli toplar olarak �iz
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(new Vector3(node.position.x, node.position.y, 0), 0.2f);

                // Kom�ularla aras�ndaki yollar� sar� �izgiyle ba�la
                if (node.neighbors != null)
                {
                    Gizmos.color = Color.yellow;
                    foreach (var neighbor in node.neighbors)
                    {
                        Gizmos.DrawLine(
                            new Vector3(node.position.x, node.position.y, 0),
                            new Vector3(neighbor.position.x, neighbor.position.y, 0)
                        );
                    }
                }
            }
        }
    }
}