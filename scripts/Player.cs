using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Player : RigidBody2D
{
	[Export] public float acceleration = 0.5f;
	[Export] public float deceleration = 2f;
	[Export] public float maxVelocity = 250f;
	[Export] public float zeroVelocityRange = 10f;

	private Vector2 thrust = new Vector2(0, -250);
	private float torque = 2500;

	public Camera2D camera;

	private Vector2 direction = Vector2.Zero;
	private string playerState = "stopped";

	public override void _Ready()
	{
		camera = GetNode<Camera2D>("../Camera2D");
	}

	public override void _Process(float delta)
	{
		if (Input.IsActionJustReleased("restart"))
		{
			GetTree().ReloadCurrentScene();
		}
	}

	public override void _IntegrateForces(Physics2DDirectBodyState state)
	{
		Rotation = (GetGlobalMousePosition() - Position).Angle() + (90 * Mathf.Pi / 180);

		if (Input.IsActionPressed("move_forward"))
		{
			AppliedForce = thrust.Rotated(Rotation);
			playerState = "moving forward";
		}
		else
		{
			AppliedForce = new Vector2();
			playerState = "slowing down";

			if (LinearVelocity.Abs().x <= zeroVelocityRange)
			{
				LinearVelocity = new Vector2(0, LinearVelocity.y);
			}
			if (LinearVelocity.Abs().y <= zeroVelocityRange)
			{
				LinearVelocity = new Vector2(LinearVelocity.x, 0);
			}
		}

		// var rotationDir = 0;
		// if (Input.IsActionPressed("move_right"))
		// 	rotationDir += 1;
		// if (Input.IsActionPressed("move_left"))
		// 	rotationDir -= 1;
		// AppliedTorque = rotationDir * torque;

		LinearVelocity = LinearVelocity.Clamped(maxVelocity);
	}

	public override void _PhysicsProcess(float delta)
	{
		Node inventoryManager = GetNode("/root/InventoryManager");
		string[] resourceTypeNames = Enum.GetNames(typeof(InventoryManager.ResourceTypes));
		string storedResourcesString = "";

		for (int i = 0; i < resourceTypeNames.Length; i++)
		{
			object dictionaryObject = inventoryManager.Get("storedResources");
			Dictionary<string, object> storedResources = dictionaryObject.GetType().GetProperties().ToDictionary(property => property.Name, property => property.GetValue(dictionaryObject));
			storedResourcesString += $"\n {resourceTypeNames[i]}: {storedResources[i.ToString()].ToString()}";
		}

		GetNode<Label>("/root/Node2D/UI/Debug info").Text =
		$"Delta time: {delta}"
		+ $"\n\nPosition: {Position}\nVelocity: {LinearVelocity}\nState: {playerState}"
		+ $"\n\nInventory"
		+ storedResourcesString;
	}
}
