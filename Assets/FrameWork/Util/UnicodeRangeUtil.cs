

[System.Flags]
public enum UnicodeRangeFilterType
{
	Num = 1 << 0,
	EngLow = 1 << 1,
	EngHigh = 1 << 2,
	Kor0 = 1 << 3, //한글 음절(가~힣)	(완성형) 
	Kor1 = 1 << 4, //한글(자음, 모음)
	Kor2 = 1 << 5, //호환용 한글(자음, 모음)	 
	CJK0 = 1 << 6, // 한중일 부수 보충
	CJK1 = 1 << 7, // 한중일 통합 한자 확장 - A
	CJK2 = 1 << 8, // 한중일 통합 한자
	CJK3 = 1 << 9, // 한중일 호환용 한자	
	CJK4 = 1 << 10, // 한중일 통합 한자 확장	 
	CJK5 = 1 << 11, // 한중일 호환용 한자 보충	 
	JPN0 = 1 << 12, // 하라가나	
	JPN1 = 1 << 13, // 가타카나		
	JPN2 = 1 << 14, // 가타카나 음성 확장
	Latin0 = 1 << 15, // 라틴 -1
	Latin1 = 1 << 16, // 라틴 문자 추가 확장, Latin extended additional
	Latin2 = 1 << 17, // 라틴 C
	Latin3 = 1 << 18, // 라틴 D
	Latin4 = 1 << 19, // 라틴 E
	Cyrillic0 = 1 << 20, // 키릴
	Cyrillic1 = 1 << 21, // 키릴
	Cyrillic2 = 1 << 22, // 키릴
	Thai0 = 1 << 23, // 타이

	Eng = EngLow | EngHigh,
	// 영문,숫자
	NumEng = Num | Eng,
	// 한글 허용된
	KorAllowed = Kor0,
	// 한글 금지된
	KorBanned = Kor1 | Kor2,
	// 모든 한글
	Kor = Kor0 | Kor1 | Kor2,
	// 모든 한중일 
	KorJpnChi = Kor | JPN0 | JPN1 | JPN2 | CJK0 | CJK1 | CJK2 | CJK3 | CJK4 | CJK5,
	KorJpnChiAllowed = KorAllowed | JPN0 | JPN1 | JPN2 | CJK0 | CJK1 | CJK2 | CJK3 | CJK4 | CJK5,

	LatinAllowed = Latin0 | Latin1 | Latin2 | Latin3 | Latin4,
	CyrillicAllowed = Cyrillic0 | Cyrillic1,
	ThaiAllowed = Thai0,
}

/// <summary>
/// 유용한 사이트
/// 유니코드 범위표: https://namu.wiki/w/%EC%9C%A0%EB%8B%88%EC%BD%94%EB%93%9C#s-4.3.1
/// 유니코드 컨버터: https://www.branah.com/unicode-converter
/// </summary>
public class UnicodeRangeUtil
{
	public struct UnicodeRange
	{
		public UnicodeRangeFilterType type;
		public int from;
		public int to;
		public UnicodeRange(int from, int to, UnicodeRangeFilterType type)
		{
			this.type = type;
			this.from = from;
			this.to = to;
		}
		public bool Contains(char c) { return from <= c && c <= to; }
	}

	static UnicodeRange[] _unicodeRanges;

	static UnicodeRange[] unicodeRanges
	{
		get
		{
			if (_unicodeRanges == null)
			{
				_unicodeRanges = new UnicodeRange[]
				{
					new UnicodeRange(0x0030, 0x0039, UnicodeRangeFilterType.Num), // 숫자
					new UnicodeRange(0x0041, 0x005a, UnicodeRangeFilterType.EngHigh), //영대문자
					new UnicodeRange(0x0061, 0x007a, UnicodeRangeFilterType.EngLow), //영소문자
					new UnicodeRange(0xAC00, 0xD7A3, UnicodeRangeFilterType.Kor0), //한글 음절(가~힣)	(완성형) 
					new UnicodeRange(0x1100, 0x11FF, UnicodeRangeFilterType.Kor1), //한글(자음, 모음)
					new UnicodeRange(0x3131, 0x318F, UnicodeRangeFilterType.Kor2), //호환용 한글(자음, 모음)	 
		
					new UnicodeRange(0x2E80, 0x2EFF, UnicodeRangeFilterType.CJK0), //한중일 부수 보충
					new UnicodeRange(0x3400, 0x4DBF, UnicodeRangeFilterType.CJK1), //한중일 통합 한자 확장 - A
					new UnicodeRange(0x4E00, 0x9FBF, UnicodeRangeFilterType.CJK2), //한중일 통합 한자
					new UnicodeRange(0xF900, 0xFAFF, UnicodeRangeFilterType.CJK3), //한중일 호환용 한자	
					new UnicodeRange(0x20000,0x2A6DF, UnicodeRangeFilterType.CJK4), //한중일 통합 한자 확장	 
					new UnicodeRange(0x2F800,0x2FA1F, UnicodeRangeFilterType.CJK5), //한중일 호환용 한자 보충	 

					new UnicodeRange(0x3040, 0x309F, UnicodeRangeFilterType.JPN0), //하라가나	 
					new UnicodeRange(0x30A0, 0x30FF, UnicodeRangeFilterType.JPN1), //가타카나	 
					new UnicodeRange(0x31F0, 0x31FF, UnicodeRangeFilterType.JPN2), //가타카나 음성 확장	 

					new UnicodeRange(0x00C0, 0x024F, UnicodeRangeFilterType.Latin0), //라틴어
					new UnicodeRange(0x1E00, 0x1EFF, UnicodeRangeFilterType.Latin1), //라틴어 확장
					new UnicodeRange(0x2C60, 0x2C7F, UnicodeRangeFilterType.Latin2), //라틴어 C
					new UnicodeRange(0xA720, 0xA7FF, UnicodeRangeFilterType.Latin3), //라틴어 D
					new UnicodeRange(0xAB30, 0xAB7F, UnicodeRangeFilterType.Latin4), //라틴어 E

					new UnicodeRange(0x0400, 0x052F, UnicodeRangeFilterType.Cyrillic0), //키릴 자모 + 키릴 자모 보충
					new UnicodeRange(0x2DE0, 0x2DFF, UnicodeRangeFilterType.Cyrillic1), //키릴 자모 확장-A
					new UnicodeRange(0xA640, 0xA69F, UnicodeRangeFilterType.Cyrillic2), //키릴 자모 확장-B

					new UnicodeRange(0x0E00, 0x0E7F, UnicodeRangeFilterType.Thai0), //키릴 자모 확장-B
				};
			}
			return _unicodeRanges;
		}
	}

	public static bool Contains(UnicodeRangeFilterType mask, char c)
	{
		for (int i = 0; i < unicodeRanges.Length; i++)
		{
			UnicodeRange range = _unicodeRanges[i];
			if (IntBitFlags.HasFlag((int)mask, (int)range.type))
				if (range.Contains(c))
					return true;
		}
		return false;
	}

}



