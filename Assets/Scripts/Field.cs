using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

public class Field//handles section of files
{
	private List<Field> fields;
	private List<string> names;//could be replaced by map for faster access
	private List<string> content;
	
	private int id;
	private int counter = 0;
	
	public Field(){
	
		fields = new List<Field>();
		names = new List<String>();
		content = new List<String>();
		id = counter; counter++;
	}
	
	
	
	
	public Field(List<String> list){	
		id = counter; counter++;
	
		this.fields = new List<Field>();
		this.names = new List<String>();
		this.content = getClone(list);
		
		for (int line = 0; line < content.Count; line ++){
			content[line] = content[line].Trim();
		}
		
		//parse content if not leaf
		if (!isLeaf()){
		
			
			//#Debug.Log("<<<<");
			for (int line = 0; line < content.Count; line ++){
				if (content[line].Length == 0) continue;
				if (content[line][0] == '<'){
					string name  = content[line].Substring(1,content[line].Length - 2);
					List<string> fieldContent  = new List<string>();
					//#Debug.Log(id + " Found <" + name + ">");
					//#Debug.Log("Looking for " + String.Format("</{0}>",name) );
					
					int depth = 1;
					while(depth > 0 ){
						line ++;
						if (line >= content.Count) Debug.LogError(id + " Index out of range: " + line + "/" + content.Count);
						if (content[line] == String.Format("</{0}>",name)) depth --;
						if (content[line] == String.Format("<{0}>",name)) depth ++;
						if (depth > 0) {
							fieldContent.Add(content[line]);
							//#Debug.Log(id + ": " + content[line]);
						}
						//#Debug.Log(depth);
					}
					
										
					//#Debug.Log(id + " End: " + content[line]);
					fields.Add(new Field(fieldContent));
					names.Add(name);
					//#Debug.Log("----");
				}else{
					//#Debug.Log("this is weird stuff: " + content[line]);
				}
			}
			//#Debug.Log(">>>>");
		}else{
			//#Debug.Log(id + " Found Content: " + getValue());
			//#Debug.Log(id + " is leaf: " + isLeaf());
		}
		getContent();
	}

    private static List<string> strArrToList(string[] content)
    {
        List<string> list = new List<string>();

        for (int i = 0; i < content.Length; i++)
        {
            list.Add(content[i]);
        }
        return list;
    }
	
	public  Field(string[] content) : this(strArrToList(content)){
		
	}

	public  Field(string serial) : this(serial.Split('\n')){

    }
	
	public int size(){
		return fields.Count;
	}
	
	private List<string> getClone(List<string> content){
		List<string> list  = new List<string>();
		
		for (int i = 0; i < content.Count; i++){
			list.Add(content[i]);
		}
		return list;
	}
	public Field getClone(){
		
		return new Field(getClone(getContent()));
	}
	
	//returns first field with specified name
	//if no such field exists, null is returned
	public Field getField(string name){
		List<Field> arr = getFields(name);
		if (arr.Count > 0)
			return arr[0];
		Debug.Log("There's no field named " + name);
		return null;
	}
	
	//returns first field with specified name
	//if no such field a new one is created and returned
	public Field atField(string name){
	
		if (getField(name) != null) return getField(name);
		return addField(name);
	}
	
	public List<Field> getFields(string name){
		
		List<Field> result = new List<Field>();
		
		for (int i = 0; i < fields.Count; i++){
			if (names[i].ToUpper() == name.ToUpper()){
				result.Add(fields[i]);
			}
		}
		
		return result;
	}
	public List<Field> getFields(){
		
		List<Field> result = new List<Field>();
		
		for (int i = 0; i < fields.Count; i++){
		
			result.Add(fields[i]);
		}
		
		return result;
	}
	public List<string> getNames(){
		
		List<string> result = new List<string>();
		
		for (int i = 0; i < fields.Count; i++){
		
			result.Add(names[i]);
		}
		
		return result;
	}
	
	public bool isLeaf()
	{
        //Debug.Log(getId() + " is leaf: " + (content.Count > 0 && content[0].Length > 0 && content[0][0] == "="));
        return content.Count > 0 && content[0].Length > 0 && content[0][0] == '=';
	
	}
	
	public bool equals(Field field){
		return getId() == field.getId();
	}
	
	public int getId()
	{
		return id;
	}
	
	public byte[] getValue()
	{
		if (isLeaf()){
			
			return Convert.FromBase64String(content[0].Substring(1,content[0].Length - 1));
		}else{
			Debug.LogWarning(id + " Tried to read value of field that is no leaf");
			return new byte[0];
		}
		
	}
	
	
	public void setValue(byte[] val){
        string str = Convert.ToBase64String(val);
        content = new List<string>();
		content.Add(String.Format("={0}", str));
	}
	
	public Field addField(string name, List<string> content){
	
		fields.Add(new Field(content));
		names.Add(name);
		return fields[fields.Count - 1];
    }
    public Field addField(string name, Field field)
    {

        fields.Add(field);
        names.Add(name);
        return fields[fields.Count - 1];
    }
    public Field addField(string name ){
	
		fields.Add(new Field());
		names.Add(name);
		return fields[fields.Count - 1];
	}
	
	public bool removeField(Field field ){//removes field if child
		for (int i = 0; i < fields.Count; i++){
			if (fields[i].equals(field)){
				fields.RemoveAt(i);
				names.RemoveAt(i);
				return true;
			}
		}
		return false;
	}
	
	public void removeField(string name){
		
		List<int> result = new List<int>();
		for (int i  = 0; i < fields.Count; i++){
			if (names[i].ToUpper() == name.ToUpper()){
				result.Add(i);
				Debug.Log("Added " + i);
			}
		}
		for (int i = result.Count - 1; i >= 0; i--){
			Debug.Log("Removing " + result[i] + "/" + fields.Count);
			fields.RemoveAt(result[i]);
			names.RemoveAt(result[i]);
		}
	}
	
	
	public string serialize(){
		return String.Join("\n",getContent().ToArray());
	}

    public override string ToString()
    {
        return serialize();
    }

    public List<string> getContent(){
		//Debug.Log("getting content of " + id);
		//FileIO.WriteFile("data/field" + id + ".txt", content);
		if (isLeaf()){
			return indent(getClone(content));
		}else{
			List<string> content = new List<string>();
			for (int i = 0; i < fields.Count; i++){
				content.Add(String.Format("<{0}>",names[i]));
				content.AddRange(fields[i].getContent());
				content.Add(String.Format("</{0}>",names[i]));
			}
			return indent(content);
		}
	}
	
	//to get a nicer looking output file
	private List<string> indent(List<string> text){
		for (int line = 0; line < text.Count; line ++){
			text[line] = "\t" + text[line];
		}
		return text;
	}
	
	// ----
	// custom functions for use with unity
	
	public void setVector3(Vector3 vec){
		atField("x").setString(vec.x.ToString());
		atField("y").setString(vec.y.ToString());
		atField("z").setString(vec.z.ToString());
	}
	public Vector3 getVector3(){
		Vector3 vec = new Vector3();
		vec.x = atField("x").getFloat();
		vec.y = atField("y").getFloat();
		vec.z = atField("z").getFloat();
		return vec;
	}public void setQuaternion(Quaternion vec ){
		atField("x").setString(vec.x.ToString());
		atField("y").setString(vec.y.ToString());
		atField("z").setString(vec.z.ToString());
		atField("w").setString(vec.w.ToString());
	}
	public Quaternion getQuaternion() {
		Quaternion vec = new Quaternion();
		vec.x = atField("x").getFloat();
		vec.y = atField("y").getFloat();
		vec.z = atField("z").getFloat();
		vec.w = atField("w").getFloat();
		return vec;
	}
	
	public void setInt(int val ){
		setString(val.ToString());
	}
	public int getInt() {
		if (getString() != ""){
			try{
				return int.Parse(getString());
			}catch{
				return 0;
			}
		}else{
			return 0;
		}
	}
	
	public void setFloat(float val){
		setString(val.ToString());
	}
	public float getFloat(){
		if (getString() != ""){
			try{
				return float.Parse(getString(),System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
			}catch{
				return 0f;
			}
		}else{
			return 0f;
		}
	}
	
	public void setBoolean(bool val){
		if (val){
			setString("true");
		}else{
			setString("false");
		}
	}
	public bool getBoolean(){
		if (getString().ToUpper() == "TRUE"){
			return true;
		}else{
			return false;
		}
	}
	
	public void setString(string str){
		setValue(Encoding.UTF8.GetBytes(str));
	}
	public string getString(){
		return Encoding.UTF8.GetString(getValue());
	}
	
}