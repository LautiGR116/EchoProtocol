import Foundation
import AVFoundation
import CoreGraphics
import CoreVideo
import ImageIO

// Local AVFoundation encoder; no new project dependency or remote upload.
struct Frame:Decodable {let file:String;let seconds:Double}
struct Cut:Decodable {let start:Double;let end:Double;let label:String}
struct Plan:Decodable {let capture:String;let output:String;let cuts:[Cut]}
let a=CommandLine.arguments
guard a.count==2 else{fatalError("encode_comparison.swift plan.json")}
let plan=try JSONDecoder().decode(Plan.self,from:Data(contentsOf:URL(fileURLWithPath:a[1])))
let folder=URL(fileURLWithPath:plan.capture,isDirectory:true)
let rows=try String(contentsOf:folder.appendingPathComponent("frames.jsonl"),encoding:.utf8).split(separator:"\n").map{try JSONDecoder().decode(Frame.self,from:Data($0.utf8))}
let output=URL(fileURLWithPath:plan.output),silent=output.deletingPathExtension().appendingPathExtension("silent.mp4")
guard !FileManager.default.fileExists(atPath:output.path) else{fatalError("Preserve existing output")}
let writer=try AVAssetWriter(outputURL:silent,fileType:.mp4)
let width=960,height=540
let input=AVAssetWriterInput(mediaType:.video,outputSettings:[AVVideoCodecKey:AVVideoCodecType.h264,AVVideoWidthKey:width,AVVideoHeightKey:height,AVVideoCompressionPropertiesKey:[AVVideoAverageBitRateKey:3_000_000]])
let adaptor=AVAssetWriterInputPixelBufferAdaptor(assetWriterInput:input,sourcePixelBufferAttributes:[kCVPixelBufferPixelFormatTypeKey as String:kCVPixelFormatType_32ARGB,kCVPixelBufferWidthKey as String:width,kCVPixelBufferHeightKey as String:height,kCVPixelBufferCGImageCompatibilityKey as String:true,kCVPixelBufferCGBitmapContextCompatibilityKey as String:true])
writer.add(input);guard writer.startWriting()else{fatalError("writer start")};writer.startSession(atSourceTime:.zero)
var cursor=0.0
var cachedPath="",cached:CGImage?=nil
for cut in plan.cuts {
 var index=rows.firstIndex{ $0.seconds>=cut.start } ?? 0
 if index>0 && rows[index].seconds>cut.start {index-=1}
 let count=Int(ceil((cut.end-cut.start)*30))
 for f in 0..<count {
  let sourceTime=cut.start+Double(f)/30
  while index+1<rows.count && rows[index+1].seconds<=sourceTime {index+=1}
  let row=rows[index]
  if row.file != cachedPath {
   let url=folder.appendingPathComponent(row.file)
   guard let src=CGImageSourceCreateWithURL(url as CFURL,nil),let img=CGImageSourceCreateImageAtIndex(src,0,nil) else{fatalError("missing image")}
   cached=img;cachedPath=row.file
  }
  let deadline=Date().addingTimeInterval(20)
  while !input.isReadyForMoreMediaData {if Date()>deadline||writer.status == .failed{fatalError("blocked writer")};usleep(1000)}
  var raw:CVPixelBuffer?
  guard let pool=adaptor.pixelBufferPool,CVPixelBufferPoolCreatePixelBuffer(nil,pool,&raw)==kCVReturnSuccess,let buffer=raw else{fatalError("buffer")}
  CVPixelBufferLockBaseAddress(buffer,[])
  let context=CGContext(data:CVPixelBufferGetBaseAddress(buffer),width:width,height:height,bitsPerComponent:8,bytesPerRow:CVPixelBufferGetBytesPerRow(buffer),space:CGColorSpaceCreateDeviceRGB(),bitmapInfo:CGBitmapInfo.byteOrder32Big.rawValue|CGImageAlphaInfo.noneSkipFirst.rawValue)!
  context.draw(cached!,in:CGRect(x:0,y:0,width:width,height:height))
  CVPixelBufferUnlockBaseAddress(buffer,[])
  guard adaptor.append(buffer,withPresentationTime:CMTime(seconds:cursor+Double(f)/30,preferredTimescale:60000)) else{fatalError("append")}
 }
 cursor+=Double(count)/30
}
input.markAsFinished();let done=DispatchSemaphore(value:0);writer.finishWriting{done.signal()};guard done.wait(timeout:.now()+45) == .success,writer.status == .completed else{fatalError("finish")}
let composition=AVMutableComposition();let v=composition.addMutableTrack(withMediaType:.video,preferredTrackID:kCMPersistentTrackID_Invalid)!
let asset=AVURLAsset(url:silent);try v.insertTimeRange(CMTimeRange(start:.zero,duration:asset.duration),of:asset.tracks(withMediaType:.video)[0],at:.zero)
let wav=AVURLAsset(url:folder.appendingPathComponent("listener.wav"));let track=composition.addMutableTrack(withMediaType:.audio,preferredTrackID:kCMPersistentTrackID_Invalid)!
let parameters=AVMutableAudioMixInputParameters(track:track);cursor=0
for cut in plan.cuts {
 let duration=Double(Int(ceil((cut.end-cut.start)*30)))/30
 let start=CMTime(seconds:cursor,preferredTimescale:60000),length=CMTime(seconds:duration,preferredTimescale:60000)
 try track.insertTimeRange(CMTimeRange(start:CMTime(seconds:cut.start,preferredTimescale:60000),duration:length),of:wav.tracks(withMediaType:.audio)[0],at:start)
 parameters.setVolumeRamp(fromStartVolume:0,toEndVolume:1,timeRange:CMTimeRange(start:start,duration:CMTime(seconds:0.008,preferredTimescale:60000)))
 parameters.setVolumeRamp(fromStartVolume:1,toEndVolume:0,timeRange:CMTimeRange(start:CMTime(seconds:cursor+duration-0.008,preferredTimescale:60000),duration:CMTime(seconds:0.008,preferredTimescale:60000)))
 cursor+=duration
}
let mix=AVMutableAudioMix();mix.inputParameters=[parameters]
let export=AVAssetExportSession(asset:composition,presetName:AVAssetExportPreset960x540)!
export.outputURL=output;export.outputFileType = .mp4;export.audioMix=mix;export.shouldOptimizeForNetworkUse=true
let end=DispatchSemaphore(value:0);export.exportAsynchronously{end.signal()};guard end.wait(timeout:.now()+60) == .success,export.status == .completed else{fatalError("export failed \(String(describing:export.error))")}
let final=AVURLAsset(url:output);print("AV evidence: \(output.path), \(final.duration.seconds)s, video \(final.tracks(withMediaType:.video).count), audio \(final.tracks(withMediaType:.audio).count); original signal gain, 8 ms edit fades only")
