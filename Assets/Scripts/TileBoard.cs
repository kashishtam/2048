using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBoard : MonoBehaviour
{
    public GameManager gameManager;
    public Tile tilePrefab;
    public TileState[] tileStates;
    private TileGrid grid;
    private List<Tile> tiles;
    private bool waiting;
    private bool gameWon;
    private void Awake(){
        grid = GetComponentInChildren<TileGrid>();
        tiles = new List<Tile>(16);
    }

    private void Update() {
        if(!waiting){
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                Move(Vector2Int.up, 0, 1, 1, 1);
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Move(Vector2Int.left, 1, 1, 0, 1);
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                Move(Vector2Int.down, 0, 1, grid.height - 2, -1);
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                Move(Vector2Int.right, grid.width - 2, -1, 0, 1);
            }
        }
    }
    public void CreateTile(){
        Tile tile = Instantiate(tilePrefab, grid.transform);
        tile.SetState(tileStates[0],2);
        tile.Spawn(grid.GetRandomEmptyCell());
        tiles.Add(tile);
    }

    public void ClearBoard(){

        foreach(var cell in grid.cells){
            cell.tile = null;
        }

        foreach(var tile in tiles){
            Destroy(tile.gameObject);
        }

        tiles.Clear();
    }

    private void Move(Vector2Int direction, int startX, int incrementX, int startY, int incrementY){
        bool changed = false;

        for (int x = startX; x >= 0 && x < grid.width; x += incrementX)
        {
            for (int y = startY; y >= 0 && y < grid.height; y += incrementY)
            {
                TileCell cell = grid.GetCell(x, y);

                if (cell.occupied) {
                    changed |= MoveTile(cell.tile, direction);
                }
            }
        }

        if(changed){
            StartCoroutine(waitForChanges());
        }
    }

    private bool MoveTile(Tile tile, Vector2Int direction){
        TileCell newCell = null;
        TileCell adjacent = grid.GetAdjacentCell(tile.cell, direction);

        while(adjacent != null){
            if(adjacent.occupied){
                if(CheckMerge(tile, adjacent.tile)){
                    Merge(tile,adjacent.tile);
                    return true;
                }
                break;
            }
            newCell = adjacent;
            adjacent = grid.GetAdjacentCell(adjacent, direction);
        }

        if(newCell != null){
            tile.MoveTo(newCell);
            return true;
        }
        return false;
    }

    private bool CheckMerge(Tile a, Tile b){
        return a.number == b.number && !b.locked;
    }

    private void Merge(Tile a, Tile b){
        tiles.Remove(a);
        a.Merge(b.cell);

        if(!gameManager.GetKeepGoing()){
            int index = FindIndex(b.state) + 1;
            int number = b.number * 2;
            if(number < 2048){
                b.SetState(tileStates[index], number);
                gameManager.SetScore(number);
            }
            else{
                b.SetState(tileStates[index-1], number);
                gameManager.SetScore(number);
                this.gameWon = true;
            }
        }
        else{
            int index = Mathf.Clamp(FindIndex(b.state) + 1, 0, tileStates.Length - 1);
            int number = b.number * 2;
            TileState newState = tileStates[index];

            b.SetState(newState, number);
            gameManager.SetScore(number);
        }
    }

    private int FindIndex(TileState state){
        for(int i =0; i<tileStates.Length; i++){
            if(tileStates[i] == state){
                return i;
            }
        }
        return -1;
    }
    private IEnumerator waitForChanges(){

        waiting = true;
        yield return new WaitForSeconds(0.1f);
        waiting = false;

        foreach(var t in tiles){
            t.locked = false;
        }
        if(tiles.Count != grid.size){
            CreateTile();
        }

        if(CheckForGameOver()){
            gameManager.GameOver();
        }

        if(!gameManager.GetKeepGoing()){
            if(gameWon){
                gameManager.GameWon();
            }
        }

    }

    private bool CheckForGameOver(){
        if(tiles.Count != grid.size){
            return false;
        }

        foreach(var tile in tiles){
            TileCell up = grid.GetAdjacentCell(tile.cell, Vector2Int.up);
            TileCell down = grid.GetAdjacentCell(tile.cell, Vector2Int.down);
            TileCell left = grid.GetAdjacentCell(tile.cell, Vector2Int.left);
            TileCell right = grid.GetAdjacentCell(tile.cell, Vector2Int.right);

            if(up!= null && CheckMerge(tile,up.tile)){
                return false;
            }
            if(down!= null && CheckMerge(tile,down.tile)){
                return false;
            }
            if(left!= null && CheckMerge(tile,left.tile)){
                return false;
            }
            if(right!= null && CheckMerge(tile,right.tile)){
                return false;
            }
        }

        return true;
    }
}
