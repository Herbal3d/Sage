# Design Notes for Sage

This document collects design and architectural notes on the Sage
program. Sage is protocol converted between the [OpenSimulator]/[SecondLife]
viewer protocol and the [Basil protocol].

Since the [Basil protocol] is build around "space servers", a Sage
instance effectivily sits in front of each [OpenSimulator] region
and serves up that region's space.

The viewer connects to all the space-server in the view.
For [OpenSimulator], that could mean many, many space-servers
because each [OpenSimulator] region is only 256x256 and thus
there is the current region, the 8 surrounding regions, and, potentially,
the 16 regions around those. That's 25 space-servers.
Longer views could be more.

TODO: Think about longer views and how to implement the distance queries
so aggrigation of space-server is possible. Does the space-server
discovery service handle this? That implies space-server registration
could inform the discovery service of aggregated collections.
This would also mean that space-server discovery includes a distance query.

## Above Sage

Each Sage instance is front ending an [OpenSimulator] region. There will
exist a service above the Sage instances that organizes the creation of
the instances (maybe only instantiated when a user is in a region) and
handles authentication/authorization for access to the Sage instances.

This service could handle space-server discovery or be an assistant service
to the space-server registration and discovery service.

## OpenSimulator Side

On the [OpenSimulator] side of the converter, [libreMetaverse] library
will be used to talk to the region server. 

This portion will:

- convert objects to GLTF format and update the asset database
- process updates and update the space-server BItem store

## Basil Side

The Basil side manages multiple connections from Basil viewers.
This means keeping multiple sets of authentication/authorization
information as well as managing multiple connections.

## Basil Connections

Basil viewer connections have a **transport** and a **protocol**.
These are managed separately and are negotiated with each
Basil viewer connection. That is, one connection could be
"WebTransport/MessagePack" while a different connection
(to a different viewer) is "/JSON". 

I am leaning toward WebTransport and MessagePack but other
forms might be used especially while debugging.

## BItem Store

The contents of the space managed by the Sage space-server is
collection of <code>BItems</code>.
This collection is essentially all of the items that are
in the space controlled by the space-server.
It includes a reference to any displayable information
for this item as well as the physical placement and motion
information. The collection of BItems does not include
any large data (meshes, sounds, ...). It only includes
pointers to any displayable.

BItems generate events. The Basil side subscribes to update
events and thus passes changes to the viewer.

The region data (object, position, ...) could be kept
in a database also so that regions can be easily restarted.
What is the tradeoff between in-memory data vs databased data?
In-memory enables the quick, transient updates (changing textures
or prim parameter changes). A backing database that is lazily updated
could work. The other end is to have a "save region data" operation
the dumps all current region data into storage. The "save region data"
would take periodic snapshots and be used at shutdown.
For [OpenSimulator] is this needed at all since region startup
dumps all region data to the viewer?



[Basil]: https://herbal3d.org/Basil
[Basil protocol]: https://herbal3d.org/BasilProtocol
[OpenSimulator]: http://opensimulator.org
[SecondLife]: https://secondlife.com
