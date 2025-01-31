using UnityEngine;
using UnityEditor.Tilemaps;

namespace UnityEditor
{
    [CustomGridBrush(true, false, false, "Coordinate Brush")]
    [CreateAssetMenu(fileName = "New Coordinate Brush", menuName = "Brushes/Coordinate Brush")]
    public class CoordinateBrush : GridBrush
    {
        public int z = 0;

        public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            Vector3Int zPosition = new Vector3Int(position.x, position.y, z);
            base.Paint(gridLayout, brushTarget, zPosition);
        }

        public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            Vector3Int zPosition = new Vector3Int(position.x, position.y, z);
            base.Erase(gridLayout, brushTarget, zPosition);
        }

        public override void FloodFill(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            Vector3Int zPosition = new Vector3Int(position.x, position.y, z);
            base.FloodFill(gridLayout, brushTarget, zPosition);
        }

        public override void BoxFill(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
        {
            Vector3Int zPosition = new Vector3Int(position.x, position.y, z);
            position.position = zPosition;
            base.BoxFill(gridLayout, brushTarget, position);
        }
    }

    [CustomEditor(typeof(CoordinateBrush))]
    public class CoordinateBrushEditor : GridBrushEditor
    {
        CoordinateBrush coordinateBrush {  get { return target as CoordinateBrush; } }

        public override void PaintPreview(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            var zPosition = new Vector3Int(position.x, position.y, coordinateBrush.z);
            base.PaintPreview(gridLayout, brushTarget, zPosition);
        }

        public override void OnPaintSceneGUI(GridLayout grid, GameObject brushTarget, BoundsInt position, GridBrushBase.Tool tool, bool executing)
        {
            base.OnPaintSceneGUI(grid, brushTarget, position, tool, executing);

            if (coordinateBrush.z != 0)
            {
                Vector3Int zPosition = new Vector3Int(position.min.x, position.min.y, coordinateBrush.z);
                BoundsInt newPosition = new BoundsInt(zPosition, position.size);
                Vector3[] cellLocals = new Vector3[]
                {
                    grid.CellToLocal(new Vector3Int(newPosition.min.x, newPosition.min.y, newPosition.min.z)),
                    grid.CellToLocal(new Vector3Int(newPosition.max.x, newPosition.min.y, newPosition.min.z)),
                    grid.CellToLocal(new Vector3Int(newPosition.max.x, newPosition.max.y, newPosition.min.z)),
                    grid.CellToLocal(new Vector3Int(newPosition.min.x, newPosition.max.y, newPosition.min.z))
                };

                Handles.color = Color.blue;

                int i = 0;
                for (int j = 0; j < cellLocals.Length; j = i++)
                {
                    Handles.DrawLine(cellLocals[j], cellLocals[i]);
                }
            }

            string labelText = "Pos: " + new Vector3Int(position.x, position.y, coordinateBrush.z);
            if (position.size.x > 1 || position.size.y > 1)
            {
                labelText += " Size: " + new Vector2Int(position.size.x, position.size.y);
            }

            GUIStyle myStyle = new GUIStyle();
            myStyle.normal.textColor = Color.white;

            Handles.Label(grid.CellToWorld(new Vector3Int(position.x, position.y, coordinateBrush.z)), labelText, myStyle);
        }
    }
}

