using System.Collections.Generic;
using UnityEngine;


/**
 * defines a cell containing a list of pointers to entities
 */
public class Cell<T> where T : class
{
    //all the entities inhabiting this cell

    public List<T> Members = new List<T>();
    
    public InvertedAABBox BBox;

    public Cell(Vector3 topleft, Vector3 botright)
    {
        BBox = new InvertedAABBox(topleft, botright);
    }
}

public class CellSpacePartition<T> 
        where T : class
{
    private List<Cell<T>> m_Cells = new List<Cell<T>>();
    //this is used to store any valid neighbors when an agent searches
    //its neighboring space
    private List<T> m_Neighbors;
    //this iterator will be used by the methods next and begin to traverse
    //through the above vector of neighbors
    private T m_curNeighbor;

    float m_fSpaceWidth  = 0f;
    float m_fSpaceHeight = 0f;

    //the number of cells the space is going to be divided up into
    int m_iNumCellsX = 0;
    int m_iNumCellsY = 0;

    float m_fCellSizeX;
    float m_fCellSizeY;

    public CellSpacePartition() { }

    public CellSpacePartition(float width, //width of 2D space
           float height, //height...
           int cellsX, //number of divisions horizontally
           int cellsY, //and vertically
           int MaxEntities)
    {  //maximum number of entities to partition
        m_fSpaceWidth = width;
        m_fSpaceHeight = height;
        m_iNumCellsX = cellsX;
        m_iNumCellsY = cellsY;
        m_Neighbors = new List<T>(MaxEntities);
        
        //calculate bounds of each cell
        m_fCellSizeX = width / cellsX;
        m_fCellSizeY = height / cellsY;
        
        //create the cells
        for (int y = 0; y < m_iNumCellsY; ++y)
        {
            for (int x = 0; x < m_iNumCellsX; ++x)
            {
                float left = x * m_fCellSizeX - width * 0.5f;
                float right = left + m_fCellSizeX;
                float top = y * m_fCellSizeY - height * 0.5f;
                float bot = top + m_fCellSizeY;

                m_Cells.Add(new Cell<T>(new Vector3(left, 0f, top), new Vector3(right,0f, bot)));
            }
        }
    }

    public void OnDebugRender()
    {
        for(int i = 0; i < m_Cells.Count; ++i)
        {
            m_Cells[i].BBox.OnDebugRender();
        }
    }     
}