using System.Collections.Generic;
using UnityEngine;

//This will be necessary for anything placed in the world and occupying grid positions
//This component is returned by grid position queries and as such a reference to this is stored in a static dictionary
public class WorldPlaceableObject : MonoBehaviour
{
    public enum WPO_Layer : short { terrain, fortification, unit}

    //Used as key for static dictionary
    public struct WorldGridPosition
    {
        public WPO_Layer layer;
        public int x;
        public int y;
        
    }

    // This getter / setter maintains a reference to this object in the static class dictionary 
    // While the value does not change, the key (grid position of this) does changes
    // setting gridPosition initially will (erroneously) remove references in dictionary at 0,0,0
    // therefore the first time the object is created it must be initialized
    private WorldGridPosition _gridPosition;
    public WorldGridPosition gridPosition
    {
        get { return _gridPosition; }
        set
        {
            placedObjects.Remove(_gridPosition);
            _gridPosition = value;
            placedObjects.Add(value, this);
        }
    }

    class GridPositionEqualityComparer : EqualityComparer<WorldGridPosition>
    {
        public override bool Equals (WorldGridPosition wgp_1, WorldGridPosition wgp_2)
        {
            return wgp_1.layer == wgp_2.layer && wgp_1.x == wgp_2.x && wgp_1.y == wgp_2.y;
        }
        public override int GetHashCode(WorldGridPosition wgp)
        {
            // This is 2^27 + 2^28 * layer + 2^13.5 * x + y
            // The idea is to span all 32 signed bits with the possible hashes
            // Min value is 2^27 - 2^31 - 11585 * 11584 - 11585 > 2^27 - 2^31 - (2^13.5)(2^13.5 - 1) - 2^13.5 = Int32.MinValue
            // In fact 2^27 - 11585 * 11584 - 11585 = 5503
            // Max value is 2^27 + 7 * 2^28 + 11585 * 11584 + 11584 < 2^27 + 2^31 - 2^28 + (2^13.5)(2^13.5 - 1) + 2^13.5 - 1 = Int32.MaxValue
            // In fact 2^27 + 7 * 2^28 + 11585 * 11584 + 11584 - 2^31 + 1 = -5503
            // This gives range intervals of [0, 15] for layers, 
            // [-11584, 11584] for x (world x in grid squares), and
            // [-17088, 17087] for y (world z in grid squares)
            // Since this will throw an exception for values outside of this range, and every value here is unique, the
            // comparison in the Equals method above is superfluous and may be set to always return true for performance reasons
            return 134217728 + 268435456 * ((int)wgp.layer - 8) + 11585 * wgp.x + wgp.y;
        }
    }
    private static readonly Dictionary<WorldGridPosition, WorldPlaceableObject> placedObjects = new Dictionary<WorldGridPosition, WorldPlaceableObject>();

    //Avoids overwriting dictionary entries at 0,0,0
    public void Initialize(WorldGridPosition InitialGridPosition)
    {
        _gridPosition = InitialGridPosition;
        placedObjects.Add(InitialGridPosition, this);
    }

    //Should remove itself from dictionary if destroyed
    public void OnDestroy()
    {
        placedObjects.Remove(_gridPosition);
    }

    public static WorldPlaceableObject GetWPOByGridPosition(WorldGridPosition wgp)
    {
        WorldPlaceableObject wpo = new WorldPlaceableObject();
        if(placedObjects.TryGetValue(wgp, out wpo))
        {
            return wpo;
        }
        else
        {
            return null;
        }
    }
    public static WorldPlaceableObject GetWPOByGridPosition(WPO_Layer layer, int x, int y)
    {
        WorldGridPosition wgp = new WorldGridPosition();
        wgp.x = x;
        wgp.y = y;
        wgp.layer = layer;
        return GetWPOByGridPosition(wgp);
    }


}
