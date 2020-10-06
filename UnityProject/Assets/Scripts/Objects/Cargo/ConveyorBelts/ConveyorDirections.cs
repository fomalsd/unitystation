using System.Collections.Generic;
using UnityEngine;

namespace Construction.Conveyors
{
	public static class ConveyorDirections
	{
		public static Dictionary<ConveyorBelt.ConveyorDirection, Vector3Int> directionsForward =
			new Dictionary<ConveyorBelt.ConveyorDirection, Vector3Int>()
			{
			{ConveyorBelt.ConveyorDirection.Up, Vector3Int.up},
			{ConveyorBelt.ConveyorDirection.Right, Vector3Int.right},
			{ConveyorBelt.ConveyorDirection.Down, Vector3Int.down},
			{ConveyorBelt.ConveyorDirection.Left, Vector3Int.left},
			{ConveyorBelt.ConveyorDirection.LeftDown, Vector3Int.down},
			{ConveyorBelt.ConveyorDirection.LeftUp, Vector3Int.up},
			{ConveyorBelt.ConveyorDirection.RightDown, Vector3Int.down},
			{ConveyorBelt.ConveyorDirection.RightUp, Vector3Int.up},
			{ConveyorBelt.ConveyorDirection.DownLeft, Vector3Int.left},
			{ConveyorBelt.ConveyorDirection.UpLeft, Vector3Int.left},
			{ConveyorBelt.ConveyorDirection.DownRight, Vector3Int.right},
			{ConveyorBelt.ConveyorDirection.UpRight, Vector3Int.right}
			};

	public static Dictionary<ConveyorBelt.ConveyorDirection, Vector3Int> directionsBackward =
		new Dictionary<ConveyorBelt.ConveyorDirection, Vector3Int>()
			{
			{ConveyorBelt.ConveyorDirection.Up, Vector3Int.down},
			{ConveyorBelt.ConveyorDirection.Right, Vector3Int.left},
			{ConveyorBelt.ConveyorDirection.Down, Vector3Int.up},
			{ConveyorBelt.ConveyorDirection.Left, Vector3Int.right},
			{ConveyorBelt.ConveyorDirection.LeftDown, Vector3Int.left},
			{ConveyorBelt.ConveyorDirection.LeftUp, Vector3Int.left},
			{ConveyorBelt.ConveyorDirection.RightDown, Vector3Int.right},
			{ConveyorBelt.ConveyorDirection.RightUp, Vector3Int.right},
			{ConveyorBelt.ConveyorDirection.DownLeft, Vector3Int.down},
			{ConveyorBelt.ConveyorDirection.UpLeft, Vector3Int.up},
			{ConveyorBelt.ConveyorDirection.DownRight, Vector3Int.down},
			{ConveyorBelt.ConveyorDirection.UpRight, Vector3Int.up}
			};
	}
}