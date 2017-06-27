function setAspectRatio(ratio){
    var re = new RegExp(/([0-9]+):([0-9]+)/);
    var result = re.exec(ratio);
    if(!result) {
        cropbox.setAspectRatio(NaN);
        return;
    }
    
    cropbox.setAspectRatio(Number(result[1]) / Number(result[2]));
    document.getElementById('CropRatio').value = ratio;
}

function initEventListeners(){
    document.getElementById("advimager_flipvertical").addEventListener('click', function(e){ 
        cropbox.getData().scaleY == 1 ? cropbox.scaleY(-1) : cropbox.scaleY(1);
    });
    document.getElementById("advimager_fliphorizontal").addEventListener('click', function(e){        
        cropbox.getData().scaleX == 1 ? cropbox.scaleX(-1) : cropbox.scaleX(1);
    });
    document.getElementById("advimager_rotateright").addEventListener('click', function(e){ 
        cropbox.rotate(90);
    });
    document.getElementById("advimager_rotateleft").addEventListener('click', function(e){        
        cropbox.rotate(-90);
    });
}

function initializeCropper(){
    var image = scForm.browser.getControl("Image");
    this.cropbox = new Cropper(image, { 
        viewMode: 1, 
        dragMode: 'move',
        autoCropArea: 0.25, 
        ready: function(e,y){
            initEventListeners();
            cropbox.zoomTo(1);
        },
        cropend: function() {
            scForm.modified = true;
        }
    });
    image.addEventListener('crop', function(e){ 
        var cropinfo = document.getElementById("CropInfo");
        var mimeType = document.getElementById("CropMimeType").value;
        cropinfo.value = cropbox.getCroppedCanvas().toDataURL(mimeType);
    });
    
}

scForm.browser.attachEvent(window, "onload", initializeCropper);
var cropbox = null;