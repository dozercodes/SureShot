#include <stdlib.h>
#include <stdio.h>

#include <Common/Base/hkBase.h>
#include <Common/Base/Ext/hkBaseExt.h>
#include <Common/Base/System/hkBaseSystem.h>
#include <Common/Base/Memory/System/Util/hkMemoryInitUtil.h>
#include <Common/Base/Memory/MemoryClasses/hkMemoryClassDefinitions.h>
#include <Common/Base/Memory/System/hkMemorySystem.h>
#include <Common/Base/Memory/Allocator/hkMemoryAllocator.h>
#include <Common/Base/Memory/Allocator/Malloc/hkMallocAllocator.h>

#include <Common/Internal/ConvexHull/hkGeometryUtility.h>
#include <Common/Internal/ConvexHull/hkPlaneEquationUtil.h>

#include <Physics/Collide/Filter/Group/hkpGroupFilter.h>
#include <Physics/Collide/Filter/Group/hkpGroupFilterSetup.h>

#include <Physics/Collide/Shape/Convex/Box/hkpBoxShape.h>
#include <Physics/Collide/Shape/Convex/Sphere/hkpSphereShape.h>
#include <Physics/Collide/Shape/Convex/Triangle/hkpTriangleShape.h>
#include <Physics/Collide/Shape/Convex/Cylinder/hkpCylinderShape.h>
#include <Physics/Collide/Shape/Convex/Capsule/hkpCapsuleShape.h>
#include <Physics/Collide/Shape/Convex/ConvexVertices/hkpConvexVerticesShape.h>
#include <Physics/Collide/Shape/Compound/Tree/Mopp/hkpMoppBvTreeShape.h>
#include <Physics/Collide/Shape/Convex/ConvexVertices/hkpConvexVerticesConnectivity.h>
#include <Physics/Collide/Shape/Convex/ConvexVertices/hkpConvexVerticesConnectivityUtil.h>
#include <Physics/Collide/Shape/Compound/Collection/ExtendedMeshShape/hkpExtendedMeshShape.h>
#include <Physics/Collide/Shape/Compound/Collection/List/hkpListShape.h>
#include <Physics/Collide/Shape/Misc/Bv/hkpBvShape.h>

#include <Physics/Collide/Dispatch/hkpAgentRegisterUtil.h>
#include <Physics/Utilities/Dynamics/Inertia/hkpInertiaTensorComputer.h>
#include <Physics/Utilities/Actions/MouseSpring/hkpMouseSpringAction.h>
#include <Physics/Utilities/Constraint/Keyframe/hkpKeyFrameUtility.h>

#include <Physics/Dynamics/World/hkpWorld.h>
#include <Physics/Dynamics/Entity/hkpRigidBody.h>
#include <Physics/Dynamics/hkpDynamics.h>
#include <Physics/Dynamics/World/hkpPhysicsSystem.h>
#include <Physics/Dynamics/World/hkpSimulationIsland.h>

#include "ContactListener.cpp"
#include "BroadphaseBorder.cpp"
#include "PhantomCallback.cpp"

hkpWorld* world;

static void HK_CALL errorReportFunction(const char* str, void*)
{
	printf("%s", str);
}

static bool allZero(float vals[], int count)
{
	for(int i = 0; i < count; ++i)
		if(vals[i] != 0)
			return false;

	return true;
}

extern "C"
{
	__declspec(dllexport) bool init_world(float gravity[], float worldSize, float collisionTolerance,
		hkpWorldCinfo::SimulationType simType, hkpWorldCinfo::SolverType solverType, bool fireCollisionCallbacks,
		bool enableDeactivation, float contactRestingVelocity)
	{
		hkMallocAllocator mallocBase;
		hkMemorySystem::FrameInfo frameInfo(0);

		hkMemoryRouter* memoryRouter;

		memoryRouter = hkMemoryInitUtil::initFreeList(&mallocBase, frameInfo);
		extAllocator::initDefault();

		if (memoryRouter == HK_NULL)
		{
			return false;
		}

		if ( hkBaseSystem::init( memoryRouter, errorReportFunction ) != HK_SUCCESS)
		{
			return false;
		}

		hkpWorldCinfo info;
		info.m_simulationType = simType;
		info.m_collisionTolerance = collisionTolerance;
		info.m_gravity = hkVector4(gravity[0], gravity[1], gravity[2]);
		info.setBroadPhaseWorldSize(worldSize);
		info.setupSolverInfo(solverType);
		info.m_fireCollisionCallbacks = fireCollisionCallbacks;
		info.m_enableDeactivation = enableDeactivation;
		info.m_contactRestingVelocity = contactRestingVelocity;

		world = new hkpWorld(info);
		hkpAgentRegisterUtil::registerAllAgents(world->getCollisionDispatcher());

		return true;
	}

	__declspec(dllexport) void set_gravity(float gravity[])
	{
		if(world == NULL)
			return;

		hkVector4 g(gravity[0], gravity[1], gravity[2]);
		world->setGravity(g);
	}

	__declspec(dllexport) void add_world_leave_callback(leaveWorldCallback callback)
	{
		world->lock();

		BroadphaseBorder* border = new BroadphaseBorder( world, callback );
		world->setBroadPhaseBorder(border);
		border->removeReference();

		world->unlock();
	}

	__declspec(dllexport) hkpShape* create_box_shape(float dim[], float convexRadius)
	{
		hkVector4 halfExtent(dim[0] / 2, dim[1] / 2, dim[2] / 2);
		return new hkpBoxShape(halfExtent, convexRadius);
	}

	__declspec(dllexport) hkpShape* create_sphere_shape(float radius)
	{
		return new hkpSphereShape(radius);
	}

	__declspec(dllexport) hkpShape* create_triangle_shape(float v0[], float v1[], float v2[], float convexRadius)
	{
		hkVector4 _v0(v0[0], v0[1], v0[2]);
		hkVector4 _v1(v1[0], v1[1], v1[2]);
		hkVector4 _v2(v2[0], v2[1], v2[2]);

		return new hkpTriangleShape(_v0, _v1, _v2, convexRadius);
	}

	__declspec(dllexport) hkpShape* create_capsule_shape(float top[], float bottom[], float radius)
	{
		hkVector4 _v0(top[0], top[1], top[2]);
		hkVector4 _v1(bottom[0], bottom[1], bottom[2]);

		return new hkpCapsuleShape(_v0, _v1, radius);
	}

	__declspec(dllexport) hkpShape* create_cylinder_shape(float top[], float bottom[], float radius, float convexRadius)
	{
		hkVector4 _v0(top[0], top[1], top[2]);
		hkVector4 _v1(bottom[0], bottom[1], bottom[2]);

		return new hkpCylinderShape(_v0, _v1, radius, convexRadius);
	}

	__declspec(dllexport) hkpShape* create_convex_shape(int numVertices, float vertices[], int stride, 
		float convexRadius)
	{
		hkStridedVertices stridedVerts;
		stridedVerts.m_numVertices = numVertices;
		stridedVerts.m_striding = stride;
		stridedVerts.m_vertices = vertices;

		hkGeometry* geometry = new hkGeometry();
		hkInplaceArrayAligned16<hkVector4,32> transformedPlanes;

		hkGeometryUtility::createConvexGeometry(stridedVerts, *geometry, transformedPlanes);

		stridedVerts.m_numVertices = geometry->m_vertices.getSize();
		stridedVerts.m_striding = sizeof(hkVector4);
		stridedVerts.m_vertices = &(geometry->m_vertices[0](0));

		hkpConvexVerticesShape* shape = new hkpConvexVerticesShape(stridedVerts, transformedPlanes, convexRadius);

		return shape;
	}

	/*__declspec(dllexport) hkpShape* create_mesh_shape(int numVertices, float vertices[], int vertexStride, 
		int numTriangles, int indices[], float convexRadius)
	{
		hkpExtendedMeshShape* mesh = new hkpExtendedMeshShape();
		mesh->setRadius(convexRadius);
		{
			hkpExtendedMeshShape::TrianglesSubpart part;

			part.m_vertexBase = vertices;
			part.m_vertexStriding = vertexStride;
			part.m_numVertices = numVertices;

			part.m_indexBase = indices;
			part.m_indexStriding = sizeof(int) * 3;
			part.m_numTriangleShapes = numTriangles;
			part.m_stridingType = hkpExtendedMeshShape::INDICES_INT32;

			mesh->addTrianglesSubpart( part );
		}

		return mesh;
	}*/

	__declspec(dllexport) hkpShape* create_mesh_shape(int numVertices, float vertices[], int vertexStride, 
		int numTriangles, int indices[], float convexRadius)
	{
		hkArray<hkpShape*> shapeArray;

		for(int i = 0; i < numTriangles * 3; i += 3)
		{
			hkpShape* triShape = create_triangle_shape(&vertices[indices[i] * 3], &vertices[indices[i + 1] * 3],
				&vertices[indices[i + 2] * 3], convexRadius);
			shapeArray.pushBack(triShape);
		}

		return new hkpListShape(&shapeArray[0], shapeArray.getSize());
	}

	__declspec(dllexport) hkpShape* create_phantom_shape(hkpShape* boundingShape,
		phantomEnterCallback enter, phantomLeaveCallback leave)
	{
		PhantomCallback* phantom = new PhantomCallback(enter, leave);
		hkpBvShape* bvShape = new hkpBvShape(boundingShape, phantom);
		phantom->removeReference();

		return bvShape;
	}

	__declspec(dllexport) hkpRigidBody* add_rigid_body(hkpShape* shape, float mass, hkpMotion::MotionType motionType, 
		hkpCollidableQualityType collideQuality, float pos[], float rot[], float linearVelocity[], float linearDamping, 
		float maxLinearVelocity, float angularVelocity[], float angularDamping, float maxAngularVelocity, float friction, 
		float restitution, float allowedPenetrationDepth, bool neverDeactivate, float gravityFactor)
	{
		world->lock();

		hkpRigidBodyCinfo bodyInfo;
		
		bodyInfo.m_shape = shape;
		bodyInfo.m_motionType = motionType;
		bodyInfo.m_position.set(pos[0], pos[1], pos[2]);
		bodyInfo.m_rotation.set(rot[0], rot[1], rot[2], rot[3]);

		if(friction >= 0)
			bodyInfo.m_friction = friction;
		if(restitution >= 0)
			bodyInfo.m_restitution = restitution;
		if(allowedPenetrationDepth >= 0)
			bodyInfo.m_allowedPenetrationDepth = allowedPenetrationDepth;
		if(collideQuality >= 0)
			bodyInfo.m_qualityType = collideQuality;
		bodyInfo.m_gravityFactor = gravityFactor;

		if(!(motionType == hkpMotion::MOTION_FIXED || motionType == hkpMotion::MOTION_KEYFRAMED))
		{
			hkpMassProperties massProperties;
			hkpInertiaTensorComputer::computeShapeVolumeMassProperties(shape, mass, massProperties);

			bodyInfo.m_mass = massProperties.m_mass;
			bodyInfo.m_centerOfMass = massProperties.m_centerOfMass;
			bodyInfo.m_inertiaTensor = massProperties.m_inertiaTensor;

			if(!allZero(linearVelocity, 3))
				bodyInfo.m_linearVelocity.set(linearVelocity[0], linearVelocity[1], linearVelocity[2]);
			if(linearDamping >= 0)
				bodyInfo.m_linearDamping = linearDamping;
			if(!allZero(angularVelocity, 3))
				bodyInfo.m_angularVelocity.set(angularVelocity[0], angularVelocity[1], angularVelocity[2]);
			if(angularDamping >= 0)
				bodyInfo.m_angularDamping = angularDamping;
			if(maxLinearVelocity >= 0)
				bodyInfo.m_maxLinearVelocity = maxLinearVelocity;
			if(maxAngularVelocity >= 0)
				bodyInfo.m_maxAngularVelocity = maxAngularVelocity;

			bodyInfo.m_enableDeactivation = !neverDeactivate;
		}

		hkpRigidBody* body = new hkpRigidBody(bodyInfo);

		world->addEntity(body);
		body->removeReference();

		shape->removeReference();

		world->unlock();

		return body;
	}

	__declspec(dllexport) void remove_rigid_body(hkpRigidBody* body)
	{
		world->removeEntity(body);
	}

	__declspec(dllexport) void add_contact_listener(hkpRigidBody* body, contactCallback cc,
		collisionStarted cs, collisionEnded ce)
	{
		world->lock();

		ContactListener* listener = new ContactListener(body);
		listener->callback = cc;
		listener->startCallback = cs;
		listener->endCallback = ce;

		world->unlock();
	}

	__declspec(dllexport) void add_force(hkpRigidBody* body, float timeStep, float force[])
	{
		hkVector4 _force(force[0], force[1], force[2]);
		body->applyForce(timeStep, _force);
	}

	__declspec(dllexport) void add_torque(hkpRigidBody* body, float timeStep, float torque[])
	{
		hkVector4 _torque(torque[0], torque[1], torque[2]);
		body->applyTorque(timeStep, _torque);
	}

	__declspec(dllexport) void set_linear_velocity(hkpRigidBody* body, float vel[])
	{
		hkVector4 velocity(vel[0], vel[1], vel[2]);
		body->setLinearVelocity(velocity);
	}

	__declspec(dllexport) void get_linear_velocity(hkpRigidBody* body, float* vel)
	{
		hkVector4 velocity = body->getLinearVelocity();
		vel[0] = velocity(0);
		vel[1] = velocity(1);
		vel[2] = velocity(2);
	}

	__declspec(dllexport) void set_angular_velocity(hkpRigidBody* body, float vel[])
	{
		hkVector4 velocity(vel[0], vel[1], vel[2]);
		body->setAngularVelocity(velocity);
	}

	__declspec(dllexport) void get_angular_velocity(hkpRigidBody* body, float* vel)
	{
		hkVector4 velocity = body->getAngularVelocity();
		vel[0] = velocity(0);
		vel[1] = velocity(1);
		vel[2] = velocity(2);
	}

	__declspec(dllexport) void apply_hard_keyframe(hkpRigidBody* body, float position[], float rotation[], float timeStep)
	{
		world->lock();

		hkVector4 pos(position[0], position[1], position[2]);
		hkQuaternion rot(rotation[0], rotation[1], rotation[2], rotation[3]);
		hkpKeyFrameUtility::applyHardKeyFrame(pos, rot, 1.0f / timeStep, body);

		world->unlock();
	}

	__declspec(dllexport) void apply_soft_keyframe(hkpRigidBody* body, float position[], float rotation[], 
		float angularPositionFactor[], float angularVelocityFactor[], float linearPositionFactor[],
		float linearVelocityFactor[], float maxAngularAcceleration, float maxLinearAcceleration, float maxAllowedDistance, 
		float timeStep)
	{
		world->lock();

		hkpKeyFrameUtility::KeyFrameInfo keyInfo;
		hkpKeyFrameUtility::AccelerationInfo accelInfo;

		keyInfo.m_position = hkVector4(position[0], position[1], position[2]);
		keyInfo.m_orientation = hkQuaternion(rotation[0], rotation[1], rotation[2], rotation[3]);
		keyInfo.m_linearVelocity = hkVector4();
		keyInfo.m_angularVelocity = hkVector4();

		accelInfo.m_angularPositionFactor = hkVector4(angularPositionFactor[0], angularPositionFactor[1],
			angularPositionFactor[2]);
		accelInfo.m_angularVelocityFactor = hkVector4(angularVelocityFactor[0], angularVelocityFactor[1],
			angularVelocityFactor[2]);
		accelInfo.m_linearPositionFactor = hkVector4(linearPositionFactor[0], linearPositionFactor[1],
			linearPositionFactor[2]);
		accelInfo.m_linearVelocityFactor = hkVector4(linearVelocityFactor[0], linearVelocityFactor[1],
			linearVelocityFactor[2]);
		accelInfo.m_maxAngularAcceleration = maxAngularAcceleration;
		accelInfo.m_maxLinearAcceleration = maxLinearAcceleration;
		accelInfo.m_maxAllowedDistance = maxAllowedDistance;
		
		hkpKeyFrameUtility::applySoftKeyFrame(keyInfo, accelInfo, timeStep, 1 / timeStep, body);

		world->unlock();
	}

	__declspec(dllexport) void get_AABB(hkpRigidBody* body, float* min, float* max)
	{
		hkAabb aabb;
		body->getCollidable()->getShape()->getAabb(body->getTransform(), 0.0f, aabb);

		hkVector4 halfExtent;
		aabb.getHalfExtents(halfExtent);
		hkVector4 center;
		aabb.getCenter(center);

		min[0] = center(0) - halfExtent(0);
		min[1] = center(1) - halfExtent(1);
		min[2] = center(2) - halfExtent(2);

		max[0] = center(0) + halfExtent(0);
		max[1] = center(1) + halfExtent(1);
		max[2] = center(2) + halfExtent(2);
	}

	__declspec(dllexport) void update(float elapsedSeconds)
	{
		hkCheckDeterminismUtil::workerThreadStartFrame(true);

		world->stepDeltaTime(elapsedSeconds);

		hkCheckDeterminismUtil::workerThreadFinishFrame();
	}

	__declspec(dllexport) void get_body_transform(hkpRigidBody* body, float* transform)
	{
		hkTransform mat;
		body->approxCurrentTransform( mat );

		mat.get4x4ColumnMajor( transform );
	}

	__declspec(dllexport) void get_body_position(hkpRigidBody* body, float* position)
	{
		hkVector4 pos = body->getPosition();
		position[0] = pos(0);
		position[1] = pos(1);
		position[2] = pos(2);
	}

	__declspec(dllexport) void get_body_rotation(hkpRigidBody* body, float* rotation)
	{
		hkQuaternion rot = body->getRotation();
		rotation[0] = rot(0);
		rotation[1] = rot(1);
		rotation[2] = rot(2);
		rotation[3] = rot(3);
	}

	__declspec(dllexport) void get_updated_transforms(int* bodyPtr, float* transformPtr, int &totalSize)
	{
		world->markForRead();

		const hkArray<hkpSimulationIsland*>& activeIslands = world->getActiveSimulationIslands();
		totalSize = 0;
		for(int i = 0; i < activeIslands.getSize(); i++)
		{
			const hkArray<hkpEntity*>& activeEntities = activeIslands[i]->getEntities();
			totalSize += activeEntities.getSize();
		}

		int count = 0;
		for(int i = 0; i < activeIslands.getSize(); i++)
		{
			const hkArray<hkpEntity*>& activeEntities = activeIslands[i]->getEntities();
			for(int j = 0; j < activeEntities.getSize(); j++, count++)
			{
				hkpRigidBody* rigidBody = static_cast<hkpRigidBody*>(activeEntities[j]);
				bodyPtr[count] = (int)rigidBody;

				hkTransform transform;
				rigidBody->approxCurrentTransform( transform );
				
				transform.get4x4ColumnMajor((transformPtr + count * 16));
			}
		}

		world->unmarkForRead();
	}

	__declspec(dllexport) void dispose()
	{
		world->removeAll();
		world->removeReference();
	}
}