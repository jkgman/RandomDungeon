﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAllowedActions
{
	bool AllowMove();
	bool AllowRotate();
	bool AllowRun();
	bool AllowAttack();
	bool AllowDodge();
}
