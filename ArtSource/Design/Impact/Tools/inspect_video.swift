import Foundation
import AVFoundation
import ImageIO
import UniformTypeIdentifiers
let a=CommandLine.arguments
let asset=AVURLAsset(url:URL(fileURLWithPath:a[1]))
let image=try AVAssetImageGenerator(asset:asset).copyCGImage(at:CMTime(seconds:Double(a[3])!,preferredTimescale:60000),actualTime:nil)
let target=CGImageDestinationCreateWithURL(URL(fileURLWithPath:a[2]) as CFURL,UTType.png.identifier as CFString,1,nil)!
CGImageDestinationAddImage(target,image,nil);guard CGImageDestinationFinalize(target) else {fatalError("PNG")}
print("duration \(asset.duration.seconds); video \(asset.tracks(withMediaType:.video).count), audio \(asset.tracks(withMediaType:.audio).count); image \(image.width)x\(image.height)")
