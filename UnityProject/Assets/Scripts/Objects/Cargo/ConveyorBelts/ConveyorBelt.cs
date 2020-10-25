using System;
using System.Collections.Generic;
using Mirror;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Profiling;

namespace Construction.Conveyors
{
[SelectionBase]
[ExecuteInEditMode]
public class ConveyorBelt : NetworkBehaviour, ICheckedInteractable<HandApply>, ISetMultitoolMaster
{
	[SerializeField] private SpriteHandler spriteHandler = null;

	private RegisterTile registerTile;
	private Vector3Int transportDirection;

	public ConveyorBeltSwitch AssignedSwitch { get; private set; }

	private Matrix Matrix => registerTile.Matrix;

	[Tooltip("Set this conveyor belt's initial direction.")]
	[SerializeField]
	private ConveyorDirection CurrentDirection = default;

	[Tooltip("Set this conveyor belt's initial status.")]
	[SerializeField]
	private ConveyorStatus CurrentStatus = default;

	Vector2Int[] searchDirs =
	{
		new Vector2Int(-1, 0), new Vector2Int(0, 1),
		new Vector2Int(1, 0), new Vector2Int(0, -1)
	};

	#region Lifecycle

	private void OnEnable()
	{
		OnStart();
	}

	// Only runs in editor - useful for updating the sprite direction
	// when the initial direction is altered via inspector.
	private void OnValidate()
	{
		RefreshSprites();
	}

	private void OnStart()
	{
		registerTile = GetComponent<RegisterTile>();
		RefreshSprites();
	}

	public override void OnStartServer()
	{
		SyncStatus( ConveyorStatus.Off);
	}

	public override void OnStartClient()
	{
		RefreshSprites();
	}

	#endregion Lifecycle

	#region Belt Operation


	public void SyncDirection(ConveyorDirection _, ConveyorDirection newValue)
	{
		CurrentDirection = newValue;
		GetPositionOffset();
		RefreshSprites();
	}

	#endregion Belt Operation

	[Server]
	public void SetBeltFromBuildMenu(ConveyorDirection direction)
	{
		SyncDirection(direction, direction);
		//Discover any neighbours:
		for (int i = 0; i < searchDirs.Length; i++)
		{
			var conveyorBelt =
				registerTile.Matrix.GetFirst<ConveyorBelt>(registerTile.LocalPosition + searchDirs[i].To3Int(), true);

			if (conveyorBelt != null)
			{
				if (conveyorBelt.AssignedSwitch != null)
				{
					conveyorBelt.AssignedSwitch.AddConveyorBelt(new List<ConveyorBelt>{this});
					conveyorBelt.SyncStatus( conveyorBelt.CurrentStatus);
					break;
				}
			}
		}
	}

	public void MoveBelt()
	{
		Profiler.BeginSample("MoveBelt");
		if (isServer) DetectItems();
		Profiler.EndSample();
	}

	public void SetSwitchRef(ConveyorBeltSwitch switchRef)
	{
		AssignedSwitch = switchRef;
	}

		/// <summary>
		/// Updates the state of this conveyor based on the state of its assigned switch.
		/// </summary>
		[Server]
		public void UpdateState()
		{
			switch (AssignedSwitch.CurrentState)
			{
				case ConveyorBeltSwitch.SwitchState.Off:
					SyncStatus(ConveyorStatus.Off);
					break;
				case ConveyorBeltSwitch.SwitchState.Forward:
					SyncStatus(ConveyorStatus.Forward);
					break;
				case ConveyorBeltSwitch.SwitchState.Backward:
					SyncStatus(ConveyorStatus.Backward);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

	private void SyncStatus( ConveyorStatus newStatus)
	{
		CurrentStatus = newStatus;
		GetPositionOffset();
		RefreshSprites();
	}

		private void RefreshSprites()
		{
			spriteHandler.ChangeSprite((int)CurrentStatus);
			var variant = (int)CurrentDirection;
			switch (variant)
			{
				case 8:
					variant = 4;
					break;
				case 9:
					variant = 5;
					break;
				case 10:
					variant = 6;
					break;
				case 11:
					variant = 7;
					break;
			}

			spriteHandler.ChangeSpriteVariant(variant);
		}


		private void GetPositionOffset()
		{
			switch (CurrentStatus)
			{
				case ConveyorStatus.Forward:
					transportDirection = ConveyorDirections.directionsForward[CurrentDirection];
					break;
				case ConveyorStatus.Backward:
					transportDirection = ConveyorDirections.directionsBackward[CurrentDirection];
					break;
				default:
					transportDirection = Vector3Int.up;
					break;
			}
		}

	private void DetectItems()
	{
		if (CurrentStatus == ConveyorStatus.Off) return;

		Profiler.BeginSample("BeltPassableCheck");
		bool pushBlocked = !Matrix.IsPassableAt(registerTile.LocalPositionServer,
			Vector3Int.RoundToInt(registerTile.LocalPositionServer + transportDirection), true);
		Profiler.EndSample();
		if (pushBlocked)
		{
			return;
		}
		Profiler.BeginSample("BeltTransport");
		foreach (var item in Matrix.Get(registerTile.LocalPositionServer, true))
		{
			if (item == null || item.gameObject == this.gameObject
			|| !item.CustomTransform || !item.CustomTransform.IsPushable)
			{
				continue;
			}

			Transport(item.CustomTransform);
		}
		Profiler.EndSample();
	}

	[Server]
	public virtual void Transport(PushPull pushPull)
	{
		pushPull.QueuePush(transportDirection.To2Int(),3f, dontInsist: true);
	}

		public enum ConveyorStatus
		{
			Off = 0,
			Forward = 1,
			Backward = 2
		}

		public enum ConveyorDirection
		{
			Up = 0,
			Down = 1,
			Left = 2,
			Right = 3,
			LeftDown = 4,
			LeftUp = 5,
			RightDown = 6,
			RightUp = 7,
			DownLeft = 8,
			UpLeft = 9,
			DownRight = 10,
			UpRight = 11
		}

		#region Interaction

		public bool WillInteract(HandApply interaction, NetworkSide side)
		{
			if (!DefaultWillInteract.Default(interaction, side)) return false;

			if (!Validations.IsTarget(gameObject, interaction)) return false;

			// Deconstruct (crowbar) and change direction (screwdriver)
			return Validations.HasUsedItemTrait(interaction, CommonTraits.Instance.Crowbar) ||
					Validations.HasUsedItemTrait(interaction, CommonTraits.Instance.Screwdriver);
		}

		public void ServerPerformInteraction(HandApply interaction)
		{
			if (Validations.HasUsedItemTrait(interaction, CommonTraits.Instance.Wrench))
			{
				//deconsruct
				ToolUtils.ServerUseToolWithActionMessages(interaction, 2f,
					"You start deconstructing the conveyor belt...",
					$"{interaction.Performer.ExpensiveName()} starts deconstructing the conveyor belt...",
					"You deconstruct the conveyor belt.",
					$"{interaction.Performer.ExpensiveName()} deconstructs the conveyor belt.",
					DeconstructBelt);
			}

			else if (Validations.HasUsedItemTrait(interaction, CommonTraits.Instance.Screwdriver)) //change direction
			{
				ToolUtils.ServerUseToolWithActionMessages(interaction, 1f,
					"You start redirecting the conveyor belt...",
					$"{interaction.Performer.ExpensiveName()} starts redirecting the conveyor belt...",
					"You redirect the conveyor belt.",
					$"{interaction.Performer.ExpensiveName()} redirects the conveyor belt.",
					ChangeDirection);
			}
		}

		private void DeconstructBelt()
		{
			Spawn.ServerPrefab(CommonPrefabs.Instance.Metal, SpawnDestination.At(gameObject), 5);
			Despawn.ServerSingle(gameObject);
		}

		private void ChangeDirection()
		{
			int count = (int)CurrentDirection + 1;

			if (count > 11)
			{
				count = 0;
			}

			// CurrentDirection = (ConveyorDirection)count;
			//
			// spriteHandler.ChangeSpriteVariant(count);

			SyncDirection((ConveyorDirection)count, (ConveyorDirection)count);
		}

		#endregion Interaction

		#region Multitool Interaction

		[SerializeField]
		private MultitoolConnectionType conType = MultitoolConnectionType.Conveyor;
		public MultitoolConnectionType ConType => conType;

		private bool multiMaster = true;
		public bool MultiMaster => multiMaster;

		public void AddSlave(object SlaveObject)
		{
		}

		#endregion Multitool Interaction
	}
}
