
var myplupload;
var sessionid = randomString();
var url =  "testupload/Upload?sid=";
var mergeurl = "testupload/zip?sid=";
var getfileinfourl =  "testupload/getFileStatus?sid=";
var applyurl =  "testupload/apply?sid=";
var clearurl =  "testupload/clearFiles?sid=";
var delefileurl =  "testupload/deleteFile?sid=";
var maxQueue = 10;
var myswiper;
var myslider;
var activieImage;
var syncing = 0;
var applying = 0;
var maxQueue = 10;
var appPath;

$(document).ready(function () {

    appPath = $("body").attr("data-url-host");
    appPath = isEndWithString(appPath, "/") ? appPath : appPath + "/";
    url =appPath + url;
    mergeurl = appPath + mergeurl;
    getfileinfourl = appPath + getfileinfourl;
    applyurl = appPath + applyurl;
    clearurl = appPath + clearurl;
    delefileurl = appPath + delefileurl;


    initSwiper();
    
    initplupload();

    initSlider();

});

function isEndWithString(sour, endStr) {
    var d = sour.length - endStr.length;
    return (d >= 0 && sour.lastIndexOf(endStr) == d);
}


function initplupload() {
    myplupload = new plupload.Uploader({
        runtimes: 'html5,html4',
        browse_button: 'pickfiles',
        container: document.getElementById('container'),
        url: url + sessionid,
        multipart: true,
        dragdrop: true,
        drop_element : "container",
        filters: {
            max_file_size: '5mb',
            mime_types: [
                { title: '图片文件', extensions: 'png,jpg,jpeg' }
            ]
        },
        init: {

            PostInit: function () {
                $("#filelist").innerHTML = "";
            },

            FilesAdded: function (uploader, files) {               
                
                controlfilecount(uploader,files);
              
            },

            UploadProgress: function (uploader, file) {
                $("#" + file.id + " b").html(file.percent + "% " + " " + plupload.formatSize(file.size).toUpperCase());
                //$("#" + file.id + " div.plupload_file_progress_bar").css("width", file.percent + "%");

                $("#" + file.id + " .file_slide_container #contentdiv .slide-wait-botton-css").text(file.percent + "% " + plupload.formatSize(file.size).toUpperCase());
              
            },

            FileUploaded: function (uploader, file, response) {

                var obj = JSON.parse(response.response);
                //console.log(obj);
                console.log(file);
                if (obj.status == 8) {
                    //$("#descitem").remove();


                    //var html = getfileHtml(file , obj , 0 );
                    //$('#filelist').append(html);

                    addImageHtml(file, obj);

                    initPanel(file );

                    setImagePreview(obj);

                } else if(obj.status==9){
                    layer.alert("压缩失败");

                    $("#" + file.id).remove();
                    myswiper.update();
                }

                //initSwiper();                              
            },

            Browse: function (uploader) {
                //console.log("browse");                
            },

            Error: function (uploader, error) {

                var file = error.file, message;
                if (file) {
                    message = error.message;
                    if (error.details) message += " (" + error.details + ")";
                    if (error.code == plupload.FILE_SIZE_ERROR) layer.alert("文档太大" + ": " + file.name);
                    if (error.code == plupload.FILE_EXTENSION_ERROR) layer.alert("格式错误。允许" + ": JPEG, PNG.");
                    file.hint = message;
                    //alert(message);
                    //$("#" + file.id).attr("class", "plupload_failed").find("a").css("display", "block").attr("title", message);
                }
                uploader.refresh();


                //console.log(" error  " + error.code + " " + error.message);
                //alert(error);
            },

        },


    });

    myplupload.init();

   
    $("#cleanfiles").click(function () {
        resetplupload();
    });

    $("#mergefiles").click(function () {
        $.ajax({
            url: mergeurl + sessionid,
            type: "Get",
            dataType: 'json',
            success: function (data) {
                console.log(data);
                if (data.Code == 200) {
                    window.location.href = data.Message;
                } else {
                    alert(data.Message);
                }

            },
            error: function (error) {
                alert(error);
            }
        });

    });

}


function showWaiting(file) {

    $("#" + file.id + " .file_slide_container .slide-image-bg-css").html("");

    $("#" + file.id + " .file_slide_container .slide-image-bg-css").html("<div class='status_wrapper'><div class='status-icon status-uploading'></div><div class='status-text'>上传中</div></div>");


}

/*
*
*/
function controlfilecount(uploader, files) {
    var delCounter = 0;
    while (uploader.files.length > maxQueue) {
        uploader.removeFile(uploader.files[maxQueue]);
        delCounter++;
    }

    var addCounter = files.length - delCounter;

    $.each(files, function (i, file) {
        //console.log("file.status=" + file.status);
        if (addCounter <= 0 || file.status != 1) return;
        addCounter--;
        var html = getfileHtml(file, null, 1);

        $("#descitem").remove();

        $("#filelist").append(html);

        myswiper.update();

        myswiper.slideTo(myswiper.slides.length - 1, false);

    });

    uploader.start();
}

function initSwiper(){
    myswiper = new Swiper('.swiper-container',{
        direction:'horizontal',    
        prevButton: '.swiper-button-prev',
        nextButton: '.swiper-button-next',
        spaceBetween: 10,
        slidesPerView: 'auto',     
        //slidesOffsetBefore: 50,
        //slidesOffsetAfter:50,
        //observer:true,
        //observeParents:true,

    });
}


function initSlider() {
   myslider =  $("#configslider").slider({
       orientation: 'vertical',
       reversed: true,
    });

   $("#configslider").on("change", function (obj) {

       if (applying) return;

       $("#configslider").blur();

       var newValue= obj.value.newValue;
       //console.log("dd=" + newValue);
        //alert(obj.value.newValue);
       $("#configvalue").text(newValue );
        var filename = $("#containerconfig").attr("data-filename");
       
        myslider.slider("disable");

        applying = 1;

        apply(newValue , filename);    

    });
}

function apply(quantity, filename) {

    console.log("ssssid=" + sessionid);

    $.ajax({
        url: applyurl + sessionid + "&filename=" + filename + "&quantity="+quantity,
        type: "Get",
        dataType: 'json',
        success: function (data) {

            applying = 0;

            myslider.slider("enable");

            //console.log(data);
            if (data.status == 8) {
                setImagePreview(data);
            } else {
                //alert(data.Message);
               // console.log(data.message);
            }
        },
        error: function (error) {

            applying = 0;

            alert(data.Message);


            myslider.slider("enable");

        }
    });

}

function resetplupload() {

    $("#cleanfiles").blur();

    //$("#filelist").html("");
    setdesHtml();

    sessionid = randomString();

    myplupload.settings.url = url + sessionid;

    //alert(myplupload.files);
    //console.log("dd="+ myplupload.files);
    myplupload.splice();
    //console.log("ee=" + myplupload.files);
    myplupload.refresh();

    $("#previewcontainer").hide();

    $("#mergefiles").attr("disabled","disabled");

    

    $.ajax({
        url: clearurl + sessionid,
        type: 'Get',
        dataType: 'json',
        success: function (data) {
            //alert('重新生成sid');
            //sessionid = randomString();
        }, error: function (error) {
            //alert('重新生成sid');
            //sessionid = randomString();
        }
    });

}


function randomString() {
    for (var t = "0123456789abcdefghiklmnopqrstuvwxyz", e = 16, i = "", n = 0; e > n; n++) {
        var a = Math.floor(Math.random() * t.length);
        i += t.substring(a, a + 1)
    }
    return i;
}


function addImageHtml(file, obj) {
    var imagehtml = "<div class='slide-image-bg-css'><div onclick=getfilerequest(this) data-fileid='" + file.id + "' data-filename='" + file.name + "'  class='imagecss' style='background:url( " + obj.thumbnailpath + ") center center no-repeat;' ></div><div class='slide-compres-css' data-fileid='" + file.id + "' data-filename='" + file.name + "' onclick=getfilerequest(this)>" + obj.rate + "</div></div>";
    var bottonhtml = "<div class='slide-botton-css' onclick=downloadpic('" + obj.minpath + "')>下载</div>";

    var html = imagehtml + bottonhtml;

    //$("#" + file.id + " .file_slide_container #contentdiv .slide-wait-botton-css").remove();
    //$("#" + file.id + " .file_slide_container #contentdiv .slide-image-bg-css").remove();


    //myswiper.update();

    //console.log("slides count="+ myswiper.slides.length);

    $("#" + file.id + " .file_slide_container #contentdiv").html(html);

    myswiper.update();

    //console.log("slides count=" + myswiper.slides.length);

    myswiper.slideTo(myswiper.slides.length-1 , false);
}

function getfileHtml(file , obj , isWaiting ) {

    var shortname;
    if (file.name.length > 16) shortname = file.name.slice(0, 9) + "..." + file.name.slice("-" + 5);
    else shortname = file.name;

    var html = "<div id="+ file.id+ "  class='swiper-slide slideitem' style='width:180px;'>";
    var slideContainer="<div  class='file_slide_container'>"
    var progressbar = "<div class='file_progressbar_bg'><div class='file_progressbar' style='width=" + file.percent + "%'></div></div>";
    var titlehtml = "<div title=" + file.name + " class='slide-title-css'>" + shortname + "</div>"
    var closehtml = "<div id='closediv' onclick=closefile('"+ file.id+"','"+file.name+"') class='slide-close-css'></div>";

    if (!isWaiting) {

        var imagehtml = "<div class='slide-image-bg-css'><div onclick=getfilerequest(this) data-fileid='" + file.id + "' data-filename='" + file.name + "'  class='imagecss' style='background:url( " + obj.thumbnailpath + ") center center no-repeat;' ></div><div class='slide-compres-css' data-fileid='" + file.id + "' data-filename='" + file.name + "' onclick=getfilerequest(this)>" + obj.rate + "</div></div>";
        var bottonhtml = "<div class='slide-botton-css' onclick=downloadpic('" + obj.minpath + "')>下载</div>";
        html += slideContainer + titlehtml + closehtml + imagehtml + bottonhtml + "</div></div>";
             

    } else {
        var waithtml = "<div id='contentdiv'><div class='slide-image-bg-css'><div class='status-wrapper'>";
        waithtml +="<div class='status-icon status-uploading'></div><div class='status-text'>上传中</div></div></div>";
        var waithtml2 = "<div class='slide-wait-botton-css' >" + plupload.formatSize(file.size).toUpperCase() + "</div></div>";
        html += slideContainer + titlehtml + closehtml + waithtml + waithtml2 + "</div></div>";
    }

    //console.log(html);

    return html;
}

function downloadpic(url) {
    window.location.href = url;
}

function getfilerequest(obj ) {
   
    console.log($(obj));

    var id = $(obj).attr("data-fileid");
    var name = $(obj).attr("data-filename");

    //alert("id="+id +" name="+name);

    $.each(myplupload.files, function (i,item) {
        if (item.id == id) {
            $("#" + item.id).removeClass("slideitem2").addClass("slideitem");
        } else {
            $("#" + item.id).removeClass("slideitem").addClass("slideitem2");
        }
    });
   

    $.ajax({
        url: getfileinfourl + sessionid +"&filename="+name,
        type: "Get",
        dataType: 'json',
        success: function (data) {
            //console.log(data);
            if (data.status == 8) {
                setImagePreview(data);
            } else {
                alert(data.message);
            }
        },
        error: function (error) {
            alert(data.message);
        }
    });
}


function setdesHtml() {

    var html = "<div id='descitem' class='swiper-slide slideitemempty'><div class='desctext'>把你的文件放到这里</div></div>";

    $("#filelist").html(html);

    myswiper.update();
}


function setImagePreview(response) {
    $("#previewcontainer").show();
    $("#pic1div").html("原文档:<b>" + response.originSize + "</b>");
    $("#pic2div").html("压缩:<b>" + response.size +"("+ response.rate + ")</b>");

    //$("#imgpic1").attr("src", response.orginpath);
    //$("#imgpic2").attr("src", response.minpath);

    $("#origin-preview").smoothZoom('destroy');
    $("#compress-preview").smoothZoom('destroy');

    $("#origin-preview").smoothZoom({
        'image_url': response.orginpath,
        'image_original_width': response.orginwidth,
        'image_original_height': response.orginheight,
        "width": '100%',
        "height": '100%',
        "responsive": true,
        "responsive_maintain_ratio": true,
        "touch_DRAG": true,
        "mouse_DRAG": true,
        "mouse_WHEEL": true,
        "mouse_WHEEL_CURSOR_POS": false,
        "mouse_DOUBLE_CLICK": false,
        "initial_ZOOM": 50,
        "zoom_MIN": 100,
        "zoom_MAX": 400,
        "pan_BUTTONS_SHOW": "NO",
        "on_ZOOM_PAN_UPDATE": function (zd, complete) { if (complete == true) syncImage("#origin-preview"); },
        "on_ZOOM_PAN_COMPLETE": function (zd) { syncImage("#origin-preview"); },
        "animation_SMOOTHNESS": 0,
        "animation_SPEED_PAN": 10,
        "animation_SPEED_ZOOM": 10,
    });

    $("#compress-preview").smoothZoom({
        'image_url': response.minpath,
        'image_original_width': response.orginwidth,
        'image_original_height': response.orginheight,
        "width": '100%',
        "height": '100%',
        "responsive": true,
        "responsive_maintain_ratio": true,
        "touch_DRAG": true,
        "mouse_DRAG": true,
        "mouse_WHEEL": true,
        "mouse_WHEEL_CURSOR_POS": false,
        "mouse_DOUBLE_CLICK": false,
        "initial_ZOOM": 50,
        "zoom_MIN": 100,
        "zoom_MAX": 400,
        "pan_BUTTONS_SHOW": "NO",
        "on_ZOOM_PAN_UPDATE": function (zd, complete) { if (complete == true) syncImage("#compress-preview"); },
        "on_ZOOM_PAN_COMPLETE": function (zd) { syncImage("#compress-preview"); },
        "animation_SMOOTHNESS": 0,
        "animation_SPEED_PAN": 10,
        "animation_SPEED_ZOOM": 10,
    });

    $("#containerconfig").attr("data-filename", response.filename);   
    $("#configvalue").text(response.quantity);        
    $("#configslider").slider({ "min": response.minQuantity, "max": response.maxQuantity });

    myslider.slider("setValue", response.quantity);

    //console.log("ss="+JSON.stringify( response));

    $("#configadd").click(function () {

        if (applying) return;


        $(this).trigger("mouseout");
        var value = myslider.slider("getValue");
        value += 1;
        //console.log("value=" + value);

        var filename = $("#containerconfig").attr("data-filename");
        var maxvalue = myslider.slider("getAttribute", "max");
        //console.log("max=" + JSON.stringify( maxvalue)+", value="+value);
        if (value <= maxvalue) {
            myslider.slider("disable");
            myslider.slider("setValue", value);
            $("#configvalue").text(value);

            applying = 1;

            apply(value, filename);
        }
    });

    $("#configsub").click(function () {
        if (applying ) return;

        $(this).trigger("mouseout");
        var value = myslider.slider("getValue");
        value -= 1;
        var filename = $("#containerconfig").attr("data-filename");
        var minvalue = myslider.slider("getAttribute", "min");
        myslider.slider("enable");
        if (value >= minvalue) {
            myslider.slider("disable");
            myslider.slider("setValue", value);
            $("#configvalue").text(value);

            applying = 1;

            apply(value, filename);
        }
    });

}


function syncImage(elem) {
    if (syncing == 1) return;
    if (!elem) return;

    
    if (activieImage && elem != "#" + activieImage) return;

    var eleto;
    if (elem == "#origin-preview") eleto = "#compress-preview";
    else eleto = "#origin-preview";


    var zoomData = $(elem).smoothZoom("getZoomData");
    //console.log(zoomData);

    syncing = 1;

    $(eleto).smoothZoom('focusTo', {
        "x": zoomData.centerX,
        "y": zoomData.centerY,
        "zoom": zoomData.ratio * 100
    });

    $(eleto + " img").css({ "left": $(elem + " img").css("left"), "top": $(elem + " img").css("top"), "width": $(elem + " img").css("width"), "height": $(elem + " img").css("height") });

    syncing = 0;
}

function initPanel( file ) {
    $("#origin-preview,#compress-preview").mouseenter(function () { activieImage = this.id; });

    $.each(myplupload.files, function (i, item) {
        if (item.id == file.id) {
            $("#" + item.id).removeClass("slideitem2").addClass("slideitem");
        } else {
            $("#" + item.id).removeClass("slideitem").addClass("slideitem2");
        }
    });

    $("#mergefiles").removeAttr( "disabled");

}


function closefile( fileid,filename) {
    //console.log("ddd=" + JSON.stringify(myplupload.files));
    // console.log("ddd=" + JSON.stringify(myplupload.total));
    //console.log("fff=" +  fileid );
    myplupload.removeFile(fileid);
    $("#" + fileid).remove();
    myswiper.updateSlidesSize();

    if (myplupload.files.length < 1) {
        setdesHtml();
    }

    resetPreview();
    //console.log("ddd="+JSON.stringify( myplupload.total));
    //alert(myplupload.total);

    if (myplupload.files.length < 1) {
        $("#mergefiles").attr("disabled", "disabled");

    } else {
        $("#mergefiles").removeAttr("disabled");
    }

    $.ajax({
        url: delefileurl + sessionid + "&filename=" + filename,
        type: 'Get',
        dataType: 'json',
        success: function (data) {

        }, error: function (data) {

        }

    });

}

function resetPreview() {
    $("#previewcontainer").hide();
}