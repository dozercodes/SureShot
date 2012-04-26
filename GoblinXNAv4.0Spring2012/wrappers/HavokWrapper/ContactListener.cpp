#include <stdlib.h>

#include <Physics/Dynamics/Entity/hkpRigidBody.h>
#include <Physics/Dynamics/Entity/hkpEntity.h>
#include <Physics/Dynamics/Collide/ContactListener/hkpContactListener.h>
#include <Physics/Dynamics/Entity/hkpEntityListener.h>

typedef void (*contactCallback)(hkpRigidBody* body1, hkpRigidBody* body2, float contactSpeed);
typedef void (*collisionStarted)(hkpRigidBody* body1, hkpRigidBody* body2);
typedef void (*collisionEnded)(hkpRigidBody* body1, hkpRigidBody* body2);

class ContactListener : public hkpContactListener, public hkpEntityListener
{
public:

	ContactListener(hkpRigidBody* body)
	{
		body->addContactListener(this);
		body->addEntityListener(this);
	}

	void contactPointCallback( const hkpContactPointEvent& evt )
	{
		if(callback != NULL)
			callback(evt.getBody(0), evt.getBody(1), evt.getSeparatingVelocity());
	}

	void collisionAddedCallback( const hkpCollisionEvent& evt )
	{
		if(startCallback != NULL)
			startCallback(evt.getBody(0), evt.getBody(1));
	}

	void collisionRemovedCallback( const hkpCollisionEvent& evt )
	{
		if(endCallback != NULL)
			endCallback(evt.getBody(0), evt.getBody(1));
	}

	void entityDeletedCallback(hkpEntity* entity)
	{
		entity->removeContactListener(this);
		entity->removeEntityListener(this);
	}

	void entityRemovedCallback(hkpEntity* entity)
	{
	}

public:

	contactCallback callback;
	collisionStarted startCallback;
	collisionEnded endCallback;
};