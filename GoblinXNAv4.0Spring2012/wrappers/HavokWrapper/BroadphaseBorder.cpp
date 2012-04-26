#include <stdlib.h>

#include <Physics/Dynamics/World/hkpWorld.h>
#include <Physics/Dynamics/Entity/hkpEntity.h>
#include <Physics/Dynamics/Entity/hkpRigidBody.h>
#include <Physics/Dynamics/World/BroadPhaseBorder/hkpBroadPhaseBorder.h>

typedef void (*leaveWorldCallback)(hkpRigidBody* body);

class BroadphaseBorder : public hkpBroadPhaseBorder
{
public:

	leaveWorldCallback callback;

	BroadphaseBorder(hkpWorld* world, leaveWorldCallback _callback) 
		: hkpBroadPhaseBorder( world )
	{
		callback = _callback;
	}

	void maxPositionExceededCallback( hkpEntity* entity )
	{
		hkpRigidBody* body = static_cast<hkpRigidBody*>(entity);

		callback(body);
	}
};