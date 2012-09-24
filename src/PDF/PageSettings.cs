/*
	   Copyright 2012 Bob Davidson
	
	   Licensed under the Apache License, Version 2.0 (the "License");
	   you may not use this file except in compliance with the License.
	   You may obtain a copy of the License at
	
	       http://www.apache.org/licenses/LICENSE-2.0
	
	   Unless required by applicable law or agreed to in writing, software
	   distributed under the License is distributed on an "AS IS" BASIS,
	   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	   See the License for the specific language governing permissions and
	   limitations under the License.
*/

using System;

namespace PageOfBob.NFountain.Plugins
{
	public class PageSettings {
		public const float PointsPerInch = 72;
		
		public float PageWidth { get; set; }
		public float PageHeight { get; set; }
		public float LeftMargin { get; set; }
		public float RightMargin { get; set; }
		public float TopMargin { get; set; }
		public float BottomMargin { get; set; }
		public float CharacterIndent { get; set; }
		public float CharacterWidth { get; set; }
		public float ParentheticalIndent { get; set; }
		public float ParentheticalWidth { get; set; }
		public float DialogIndent { get; set; }
		public float DialogWidth { get; set; }
		public float CharacterWidthCorrection { get; set; }
		
		public float PageTitleDistanceFromTop { get; set; }
		public float AddressDistanceFromBottomMargin { get; set; }
		public float AddressIndent { get; set; }
		
		public float EffectiveCharWidth { get; set; }
		public float EffectiveCharHeight { get; set; }
		public float FontSize { get; set; }
		
		public float PageNumberDistanceTop { get; set; }
		
		public PageSettings() {
			PageWidth = 8.5f;
			PageHeight = 11f;
			LeftMargin = 1.5f;
			RightMargin = 1f;
			TopMargin = 1f;
			BottomMargin = 1f;
			CharacterIndent = 2.2f;
			CharacterWidth = 1.9f;
			ParentheticalIndent = 1.6f;
			ParentheticalWidth = 1.9f;
			DialogIndent = 1f;
			DialogWidth = 3.5f;
			CharacterWidthCorrection = 1.72f;
			
			PageNumberDistanceTop = 0.5f;
			
			PageTitleDistanceFromTop = 2.5f;
			AddressDistanceFromBottomMargin = 1.0f;
			AddressIndent = 3f;
			
			FontSize = 12f;
			EffectiveCharWidth = 7.21f;
			EffectiveCharHeight = 12f;
		}
		
		public static float InPoints(float inches) {
			return inches * PointsPerInch;
		}
		
		public float WritablePageWidth() { return PageWidth - LeftMargin - RightMargin; }
	}
}
