# 2D cloth simulation in godot using verlet integration

Written in C#, this little repo has a mostly proof of concept 2d cloth simulator. It's still missing collision, which is needed to make this usable in most cases. Use cases include capes, flags, and I'm sure many more. It renders a Polygon2D from simulated particles that are turned into a mesh by finding the convex hull of the points. I haven't even considered performance yet, which I'm sure will need to be improved.

## Use
Clone the dir into your scripts folder, and attatch the Cloth.cs script to a Polygon2D node. You should be set. Feel free to contribute or submit an issue.

![Example](cloth2d.gif)