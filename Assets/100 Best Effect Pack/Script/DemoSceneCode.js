var EffectNames = ["BloodEffect1","BloodEffect2","BloodEffect3","BloodEffect4","BloodEffect5","DarkEffect1","DarkEffect2","DarkEffect3","DarkEffect4","DarkEffect5","DirtyEffect1","DirtyEffect2","DirtyEffect3","DirtyEffect4","DirtyEffect5","ExplosionEffect1","ExplosionEffect2","ExplosionEffect3","ExplosionEffect4","ExplosionEffect5","FireEffect1","FireEffect2","FireEffect3","FireEffect4","FireEffect5","FireEffect6","FireEffect7","FireEffect8","FireEffect9","FireEffect10","FlameEmissionEffect1","FlameEmissionEffect2","FlameEmissionEffect3","FlameEmissionEffect4","FlameEmissionEffect5","HolyEffect1","HolyEffect2","HolyEffect3","HolyEffect4","HolyEffect5","IceEffect1","IceEffect2","IceEffect3","IceEffect4","IceEffect5","Kunai","Kunai2","Kunai3","Kunai4","Kunai5","Kunai6","Kunai7","Kunai8","Kunai9","Kunai10","LightningEffect1","LightningEffect2","LightningEffect3","LightningEffect4","LightningEffect5","MedicalEffect1","MedicalEffect2","MedicalEffect3","MedicalEffect4","MedicalEffect5","PoisonEffect1","PoisonEffect2","PoisonEffect3","PoisonEffect4","PoisonEffect5","PortalEffect","PortalEffect2","PortalEffect3","PortalEffect4","PortalEffect5","SwampEffect1","SwampEffect2","SwampEffect3","SwampEffect4","SwampEffect5","OtherMagicEffect1","OtherMagicEffect2","OtherMagicEffect3","OtherMagicEffect4","OtherMagicEffect5","OtherMagicEffect6","OtherMagicEffect7","OtherMagicEffect8","OtherMagicEffect9","OtherMagicEffect10","OtherMagicEffect11","OtherMagicEffect12","OtherMagicEffect13","OtherMagicEffect14","OtherMagicEffect15","OtherMagicEffect16","OtherMagicEffect17","OtherMagicEffect18","OtherMagicEffect19","OtherMagicEffect20"];
var Effect2Names = ["BloodEffect5"];
var Effect = new Transform[100];
var Text1 : GUIText;
var i : int = 0;
var a : int = 0;

function Start(){var obj = Instantiate(Effect[i], Vector3(0,5,0),Quaternion.identity);}

function Update () {

	Text1.text = i+1 + ":" +EffectNames[i];
	
	if(Input.GetKeyDown(KeyCode.Z))
	{
		if(i<=0)
			i= 99;

		else
			i--;
			
		for(a = 0 ; a < Effect2Names.length ; a++)
		{
			if(EffectNames[i] == Effect2Names[a])
			{
				var obz = Instantiate(Effect[i], Vector3(0,0.01,0),Quaternion.identity);
				break;
			}
		}
		if(a++ == Effect2Names.length)
			var obz2 = Instantiate(Effect[i], Vector3(0,5,0),Quaternion.identity);
	}
	
	if(Input.GetKeyDown(KeyCode.X))
	{
		if(i< 99)
			i++;

		else
			i=0;
			
		for(a = 0 ; a < Effect2Names.length ; a++)
		{
			if(EffectNames[i] == Effect2Names[a])
			{
				var obx = Instantiate(Effect[i], Vector3(0,0.01,0),Quaternion.identity);
				break;
			}
		}
		if(a++ == Effect2Names.length)
			var obx2 = Instantiate(Effect[i], Vector3(0,5,0),Quaternion.identity);
	}
	
	if(Input.GetKeyDown(KeyCode.C))
	{
	
		for(a = 0 ; a < Effect2Names.length ; a++)
		{
			if(EffectNames[i] == Effect2Names[a])
			{
				var obc = Instantiate(Effect[i], Vector3(0,0.01,0),Quaternion.identity);
				break;
			}
		}
		if(a++ == Effect2Names.length)
			var obc2 = Instantiate(Effect[i], Vector3(0,5,0),Quaternion.identity);
	}
}