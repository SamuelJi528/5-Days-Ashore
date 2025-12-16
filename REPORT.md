# Challenges & Fixes

## Follow camera
- Problem: The camera did not stay locked on the player and swung around wildly while moving.
- Fix: IsoFollowCamera caches the player in Awake, then LateUpdate drives a smoothed boom toward the pivot. The blend is handled by Vector3.SmoothDamp, and we clamp yaw rotations to the rotateStep increments so the rig stays centred on the avatar.
```csharp
Vector3 pivot = GetPivot(target) + pivotOffset;
Vector3 idealPos = pivot + Quaternion.Euler(pitch, yaw, 0f) * Vector3.back * distance;
transform.position = Vector3.SmoothDamp(transform.position, idealPos, ref _vel, damping);
transform.rotation = Quaternion.LookRotation(pivot - transform.position, Vector3.up);
```

## Inventory placement
- Problem: Loot from nodes and enemies often skipped the hotbar or vanished instead of landing in a slot.
- Fix: InventoryManager.AddItem first calls StackIntoExisting for all slots, then routes hotbar-friendly items through PlaceInEmptySlots(0, hotbarSize) before falling back to the backpack. This guarantees every drop either stacks or finds an empty slot.
```csharp
amount = StackIntoExisting(itemToAdd, amount, 0, inventorySlots.Count);
if (itemToAdd.displayInHotbar)
	amount = PlaceInEmptySlots(itemToAdd, amount, 0, hotbarEndIndex);
if (amount > 0)
	amount = PlaceInEmptySlots(itemToAdd, amount, backpackStartIndex, inventorySlots.Count);
```

## Sword damage
- Problem: The sword never hurt enemies even when the animation played.
- Fix: PlayerAttack.TryAttack triggers Hit() immediately. Hit() performs Physics.OverlapSphere, fetches EnemyHealth via GetComponentInParent, and applies TakeDamage. EnemyHealth maxHealth values were increased so crowds last longer after the new AoE hit check.
```csharp
Collider[] hits = Physics.OverlapSphere(transform.position, attackRadius);
foreach (var h in hits)
{
	EnemyHealth eh = h.GetComponentInParent<EnemyHealth>();
	if (eh) eh.TakeDamage(attackDamage);
}
```

## Enemy pathfinding
- Problem: Enemies could spawn off the NavMesh and freeze in place.
- Fix: EnemyAI.Start calls NavMesh.SamplePosition around the spawn point, and when a hit is found the agent is warped onto the mesh so SetDestination works immediately.
```csharp
NavMeshHit hit;
if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
	agent.Warp(hit.position);
```

## Player stamina and sprinting
- Problem: Sprinting kept draining stamina even after the player stopped moving.
- Fix: PlayerControl.Update checks stats.CurrentStamina before applying the sprint multiplier. When stamina hits zero we set isExhausted and skip sprint drain until the regen branch refills above 15f.
```csharp
if (stats.CurrentStamina <= 0f)
	isExhausted = true;
bool isSprinting = sprintInput && isMoving && !isExhausted && stats;
if (isSprinting)
	stats.DrainStamina(staminaCost);
else
	stats.RegenStamina(staminaRegen);
```

## Gathering resources
- Problem: Players could mash E without the right tool and nothing explained why the node stayed up.
- Fix: PlayerProximityDetector.PerformGathering bails if focusedNode.HasCorrectTool() fails, and ResourceNode.GatherResource double-checks HasCorrectTool before dropping loot so the prompt only fires when the player is equipped correctly.
```csharp
if (focusedNode == null || !focusedNode.HasCorrectTool())
{
	isGathering = false;
	yield break;
}
```

## Day and night cycle
- Problem: Lighting and music changed too abruptly when the clock flipped to night.
- Fix: DayNightCycle.Update drives UpdateSunAndAmbient and UpdateClock each frame; these functions interpolate sun intensity, ambient/reflection lighting, and call MusicManager.SetDayState when the hour crosses the day/night threshold, delivering a continuous transition.
```csharp
RenderSettings.ambientIntensity = Mathf.Lerp(nightAmbientIntensity, dayAmbientIntensity, dayCurve);
sun.intensity = Mathf.Lerp(nightIntensity, maxIntensity, dayCurve);
if (isDay != lastIsDay) musicManager.SetDayState(isDay);
```
