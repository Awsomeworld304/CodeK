# RAILS
Rails are good for things like sliding on them.
It's not too hard to add them but it needs some explaining,
## Adding the rail.
First start by making an empty game object and giving it the 'Rail' tag.
Next add a Spline component. This will add some points and a line.
Next add a Spline Mesh tiling component. 
### Configuring the rail.
The mesh for the tiling should be a Cylinder shown in the screenshot.
The material should be 'Base' and X and Y rotation to 90.
Make sure 'Generate Collider', 'Update in Play Mode', and 'Curve Space' are enabled.
Keep the mode to 'Stretch to Interval' or else the mesh will not be made and the player won't be able to use it.
Make sure to set the tags of the meshes to 'Rail' as well.
You can add as many nodes as you want but be careful with them.
## Adding Honing
There are two ways to do this, one with a point on each end, or points along the whole rail.
### Method One (Segmented Points)
Start by adding the 'Example Slower' to your rail.
Set the 'Spacing' to 4.
drag the 'HomingCancel' prefab into the prefab slot.
Homing should work.
#### Making the Homing
If you couldn't find the prefab, you can make it yourself.
Start by making an empty.
Give it the 'HomingTarget' tag and set the 'Obj' layer.
Next drag it out to your assets cabinet and it will be a prefab.
Drag that prefab into the slot on the script of your rail.
You can delete the one from the Hierarchy if you want.