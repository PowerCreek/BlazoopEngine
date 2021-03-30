console.log("loaded")

function SetParent(parent, child){
    console.log(parent)
    console.log(child)
    parent.appendChild(child)
}

function SetStyles(elementId, styleKey, styleValue){
    document.getElementById(elementId).style[styleKey]=styleValue

}

function SetStylesByReference(element, styleKey, styleValue){
    element.style[styleKey] = styleValue
}

function ModifyOnWheelEvent(element){
    console.log((element))
    console.log(document.getElementById(element))
    document.getElementById(element).onwheel = (e)=>{
        e.preventDefault();
    }
}

function ModifyOnWheelEventOnInit(event, element){
    event.preventDefault()
}