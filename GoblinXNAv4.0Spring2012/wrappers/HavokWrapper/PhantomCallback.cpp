#include <stdlib.h>

#include <Physics/Collide/Shape/Misc/PhantomCallback/hkpPhantomCallbackShape.h>

#include <Physics/Dynamics/Entity/hkpRigidBody.h>

typedef void (*phantomEnterCallback)(hkpRigidBody* body);
typedef void (*phantomLeaveCallback)(hkpRigidBody* body);

class PhantomCallback : public hkpPhantomCallbackShape
{
public:

	phantomEnterCallback enterEvent;
	phantomLeaveCallback leaveEvent;

	PhantomCallback(phantomEnterCallback enter, phantomLeaveCallback leave)
	{
		enterEvent = enter;
		leaveEvent = leave;
	}

	virtual void phantomEnterEvent( const hkpCollidable* collidableA, const hkpCollidable* collidableB, 
		const hkpCollisionInput& env )
	{
		if(enterEvent != NULL)
		{
			hkpRigidBody* owner = hkpGetRigidBody(collidableB);
			enterEvent(owner);
		}
	}

	// hkpPhantom interface implementation
	virtual void phantomLeaveEvent( const hkpCollidable* collidableA, const hkpCollidable* collidableB )
	{
		if(leaveEvent != NULL)
		{
			hkpRigidBody* owner = hkpGetRigidBody(collidableB);
			leaveEvent(owner);
		}
	}
};