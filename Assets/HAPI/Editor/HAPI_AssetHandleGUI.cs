/*
 * PROPRIETARY INFORMATION.  This software is proprietary to
 * Side Effects Software Inc., and is not to be reproduced,
 * transmitted, or disclosed in any way without written permission.
 *
 * Produced by:
 *      Side Effects Software Inc
 *		123 Front Street West, Suite 1401
 *		Toronto, Ontario
 *		Canada   M5J 2M2
 *		416-504-9876
 *
 * COMMENTS:
 * 
 */

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

using HAPI;

/// <summary>
/// 	GUI companion to <see cref="HAPI_Asset"/>.
/// </summary>
public partial class HAPI_AssetGUI : Editor 
{	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public	
	public void OnSceneGUI() 
	{
		Event current_event = Event.current;		
		
		if ( current_event.isKey )
		{
			if ( current_event.keyCode == KeyCode.W )
				myManipMode = XformManipMode.Translate;
			
			else if ( current_event.keyCode == KeyCode.E )
				myManipMode = XformManipMode.Rotate;
			
			else if ( current_event.keyCode == KeyCode.R )
				myManipMode = XformManipMode.Scale;
		}
		
		if ( myObjectControl == null )
			return;
		
		HAPI_HandleInfo[] handleInfos = myObjectControl.prHandleInfos;
		
		if ( handleInfos == null )
			return;				
		
		for ( int ii = 0; ii < handleInfos.Length; ++ii )
		{
			HAPI_HandleInfo handleInfo = handleInfos[ ii ];
			if ( handleInfo.typeName == "xform" )
			{
				float tx = 0, ty = 0, tz = 0;
				float rx = 0, ry = 0, rz = 0;
				float sx = 1, sy = 1, sz = 1;
				HAPI_RSTOrder rstOrder = HAPI_RSTOrder.SRT;
				HAPI_XYZOrder xyzOrder = HAPI_XYZOrder.XYZ;
				
				HAPI_HandleBindingInfo[] bindingInfos = myObjectControl.prHandleBindingInfos[ ii ];
												
				if ( myTranslateParmId == -1 ||
					 myRotateParmId == -1 ||
					 myScaleParmId == -1 ||
					 myRstOrderParmId == -1 ||
					 myXyzOrderParmId == -1 )
				{
					foreach ( HAPI_HandleBindingInfo bindingInfo in bindingInfos )
					{
						string parm_name = bindingInfo.handleParmName;
						if ( parm_name == "tx" )
							myTranslateParmId = bindingInfo.assetParmId;							
																												
						else if ( parm_name == "rx" )
							myRotateParmId = bindingInfo.assetParmId;
																		
						else if ( parm_name == "sx" )
							myScaleParmId = bindingInfo.assetParmId;
						
						else if ( parm_name == "trs_order" )
							myRstOrderParmId = bindingInfo.assetParmId;
						
						else if ( parm_name == "xyz_order" )
							myXyzOrderParmId = bindingInfo.assetParmId;
					}
				}
				
				if ( myTranslateParmId > 0 )
				{
					HAPI_ParmInfo parmInfo = myObjectControl.prParms[ myTranslateParmId ];
					tx = parmInfo.floatValue[0];
					ty = parmInfo.floatValue[1];
					tz = parmInfo.floatValue[2];
				}
				
				if ( myRotateParmId > 0 )
				{
					HAPI_ParmInfo parmInfo = myObjectControl.prParms[ myRotateParmId ];
					rx = parmInfo.floatValue[0];
					ry = parmInfo.floatValue[1];
					rz = parmInfo.floatValue[2];
				}
				
				if ( myScaleParmId > 0 )
				{
					HAPI_ParmInfo parmInfo = myObjectControl.prParms[ myScaleParmId ];
					sx = parmInfo.floatValue[0];
					sy = parmInfo.floatValue[1];
					sz = parmInfo.floatValue[2];
				}
				
				if ( myRstOrderParmId > 0 )
				{
					HAPI_ParmInfo parmInfo = myObjectControl.prParms[ myRstOrderParmId ];
					rstOrder = (HAPI_RSTOrder) parmInfo.intValue[0];
				}
				
				if ( myXyzOrderParmId > 0 )
				{
					HAPI_ParmInfo parmInfo = myObjectControl.prParms[ myXyzOrderParmId ];
					xyzOrder = (HAPI_XYZOrder) parmInfo.intValue[0];
				}				
				
				HAPI_TransformEuler xform = new HAPI_TransformEuler(true);				
				
				//This bit is a little tricky.  We will eventually call Handle.PositionHandle
				//or Handle.RotationHandle to display the translation and rotation handles.
				//These function take a translation parameter and a rotation parameter in 
				//order to display the handle in its proper location and orientation.  
				//These functions have an assumed order that it will put the rotation
				//and translation back together.  Depending whether the order of translation
				//and roation matches that of the rstOrder setting, we may, or may not
				//need to convert the translation parameter for use with the handle.
				if( rstOrder == HAPI_RSTOrder.TSR || rstOrder == HAPI_RSTOrder.STR || rstOrder == HAPI_RSTOrder.SRT )
				{
					xform.position[0] = tx;
					xform.position[1] = ty;
					xform.position[2] = tz;
					xform.rotationeEuler[0] = rx;
					xform.rotationeEuler[1] = ry;
					xform.rotationeEuler[2] = rz;
					xform.scale[0] = 1;
					xform.scale[1] = 1;
					xform.scale[2] = 1;
					xform.rotationOrder = (int) xyzOrder;
					xform.rstOrder = (int) rstOrder;
				}
				else
				{
					xform.position[0] = 0;
					xform.position[1] = 0;
					xform.position[2] = 0;
					xform.rotationeEuler[0] = rx;
					xform.rotationeEuler[1] = ry;
					xform.rotationeEuler[2] = rz;
					xform.scale[0] = 1;
					xform.scale[1] = 1;
					xform.scale[2] = 1;
					xform.rotationOrder = (int) xyzOrder;
					xform.rstOrder = (int) rstOrder;
				}
				
				HAPI_Host.convertTransform( ref xform, (int) HAPI_RSTOrder.SRT, (int) HAPI_XYZOrder.ZXY );
				
				Handles.matrix = myObjectControl.transform.localToWorldMatrix;	
				
				Vector3 translate;
				
				if( rstOrder == HAPI_RSTOrder.TSR || rstOrder == HAPI_RSTOrder.STR || rstOrder == HAPI_RSTOrder.SRT )
					translate = new Vector3( xform.position[ 0 ], xform.position[ 1 ], xform.position[ 2 ] );								
				else
					translate = new Vector3( tx, ty, tz );
				
				Quaternion rotate = Quaternion.Euler( xform.rotationeEuler[ 0 ], xform.rotationeEuler[ 1 ], 
													  xform.rotationeEuler[ 2 ] );				
				Vector3 scale = new Vector3( sx, sy, sz);
				
				if ( myManipMode == XformManipMode.Translate )
				{
					if ( myTranslateParmId > 0)
					{
						Vector3 newPos = Handles.PositionHandle( translate, rotate );
						
						if ( rstOrder == HAPI_RSTOrder.TSR 
							 || rstOrder == HAPI_RSTOrder.STR 
							 || rstOrder == HAPI_RSTOrder.SRT )
						{
							xform.position[0] = newPos[0];
							xform.position[1] = newPos[1];
							xform.position[2] = newPos[2];							
							HAPI_Host.convertTransform( ref xform, (int) rstOrder, (int) xyzOrder );
							newPos.x = xform.position[0];
							newPos.y = xform.position[1];
							newPos.z = xform.position[2];
						}
						
						myObjectControl.prParms[ myTranslateParmId ].floatValue[ 0 ] = newPos.x;
						myObjectControl.prParms[ myTranslateParmId ].floatValue[ 1 ] = newPos.y;
						myObjectControl.prParms[ myTranslateParmId ].floatValue[ 2 ] = newPos.z;
						
					}
				}
				else if ( myManipMode == XformManipMode.Rotate )
				{
					if ( myRotateParmId > 0 )
					{
						Quaternion newRotQuat = Handles.RotationHandle( rotate, translate );
						
						Vector3 newRot = newRotQuat.eulerAngles;
						
						xform.position[0] = 0;
						xform.position[1] = 0;
						xform.position[2] = 0;
						xform.rotationeEuler[0] = newRot.x;
						xform.rotationeEuler[1] = newRot.y;
						xform.rotationeEuler[2] = newRot.z;
						xform.scale[0] = 1;
						xform.scale[1] = 1;
						xform.scale[2] = 1;
						xform.rotationOrder = (int) HAPI_XYZOrder.ZXY;
						xform.rstOrder = (int) HAPI_RSTOrder.SRT;
						
						HAPI_Host.convertTransform( ref xform, (int) rstOrder, (int) xyzOrder );
						
						myObjectControl.prParms[ myRotateParmId ].floatValue[ 0 ] = xform.rotationeEuler[ 0 ];
						myObjectControl.prParms[ myRotateParmId ].floatValue[ 1 ] = xform.rotationeEuler[ 1 ];
						myObjectControl.prParms[ myRotateParmId ].floatValue[ 2 ] = xform.rotationeEuler[ 2 ];
					}					
				}
				else if ( myManipMode == XformManipMode.Scale )
				{
					if( myScaleParmId > 0 )
					{
						Vector3 newScale = Handles.ScaleHandle( scale, translate, rotate, 1.0f );
						myObjectControl.prParms[ myScaleParmId ].floatValue[ 0 ] = newScale.x;
						myObjectControl.prParms[ myScaleParmId ].floatValue[ 1 ] = newScale.y;
						myObjectControl.prParms[ myScaleParmId ].floatValue[ 2 ] = newScale.z;							
					}
				}
				
									
			}
		}
		
		if ( GUI.changed )
			myObjectControl.build();
		
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private
	
	private enum XformManipMode 
	{
		Translate = 0,
		Rotate,
		Scale
	}
	
	private XformManipMode 		myManipMode 				= XformManipMode.Translate;	
	
	private int myTranslateParmId = -1;
	private int myRotateParmId = -1;
	private int myScaleParmId = -1;
	private int myRstOrderParmId = -1;
	private int myXyzOrderParmId = -1;
					
	
}