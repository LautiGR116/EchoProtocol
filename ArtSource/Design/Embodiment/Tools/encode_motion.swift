import Foundation
import AVFoundation
import CoreGraphics
import CoreVideo
import ImageIO
import UniformTypeIdentifiers

// Local evidence encoder using macOS's existing AVFoundation. No game runtime
// dependency, remote media upload, or computer-input automation.
let args = CommandLine.arguments
guard args.count == 4 else { fatalError("Usage: encode_motion.swift route-directory output.mp4 decoded-first-frame.png") }
let folder = URL(fileURLWithPath: args[1], isDirectory: true)
let destination = URL(fileURLWithPath: args[2])
guard !FileManager.default.fileExists(atPath: destination.path) else { fatalError("Preserve existing video") }
let csv = try String(contentsOf: folder.appendingPathComponent("character-motion.csv"), encoding: .utf8)
let rows = csv.split(separator: "\n").dropFirst().map { $0.split(separator: ",", omittingEmptySubsequences: false).map(String.init) }
guard let first = rows.first else { fatalError("No actual motion frames") }
func picture(_ row: [String]) -> CGImage {
    let path = folder.appendingPathComponent("character-motion/" + String(format: "%05d", Int(row[0])!) + ".png")
    guard let source = CGImageSourceCreateWithURL(path as CFURL, nil), let image = CGImageSourceCreateImageAtIndex(source, 0, nil) else { fatalError("Missing PNG " + path.path) }
    return image
}
let initial = picture(first), width = initial.width, height = initial.height
let writer = try AVAssetWriter(outputURL: destination, fileType: .mp4)
let input = AVAssetWriterInput(mediaType: .video, outputSettings: [AVVideoCodecKey: AVVideoCodecType.h264, AVVideoWidthKey: width, AVVideoHeightKey: height, AVVideoCompressionPropertiesKey: [AVVideoAverageBitRateKey: 5_000_000, AVVideoProfileLevelKey: AVVideoProfileLevelH264HighAutoLevel]])
input.expectsMediaDataInRealTime = false
let adaptor = AVAssetWriterInputPixelBufferAdaptor(assetWriterInput: input, sourcePixelBufferAttributes: [kCVPixelBufferPixelFormatTypeKey as String: kCVPixelFormatType_32ARGB, kCVPixelBufferWidthKey as String: width, kCVPixelBufferHeightKey as String: height, kCVPixelBufferCGImageCompatibilityKey as String: true, kCVPixelBufferCGBitmapContextCompatibilityKey as String: true])
writer.add(input)
guard writer.startWriting() else { fatalError("Writer failed " + String(describing: writer.error)) }
writer.startSession(atSourceTime: .zero)
var presentation = 0.0, previous = Double(first[1])!, cutCount = 0
for (index, row) in rows.enumerated() {
    let real = Double(row[1])!
    if index > 0 {
        let difference = real - previous
        // Keep captured movement timing. Cuts omit uncaptured travel/wait gaps.
        if difference > 0.5 { presentation += 1.0 / 15.0; cutCount += 1 }
        else { presentation += max(0.001, difference) }
    }
    previous = real
    let deadline = Date().addingTimeInterval(10)
    while !input.isReadyForMoreMediaData {
        if Date() > deadline || writer.status == .failed { fatalError("Encoding blocked " + String(describing: writer.error)) }
        usleep(1000)
    }
    var buffer: CVPixelBuffer?
    guard let pool = adaptor.pixelBufferPool, CVPixelBufferPoolCreatePixelBuffer(nil, pool, &buffer) == kCVReturnSuccess, let pixels = buffer else { fatalError("Pixel pool failed") }
    CVPixelBufferLockBaseAddress(pixels, [])
    guard let context = CGContext(data: CVPixelBufferGetBaseAddress(pixels), width: width, height: height, bitsPerComponent: 8, bytesPerRow: CVPixelBufferGetBytesPerRow(pixels), space: CGColorSpaceCreateDeviceRGB(), bitmapInfo: CGBitmapInfo.byteOrder32Big.rawValue | CGImageAlphaInfo.noneSkipFirst.rawValue) else { fatalError("Pixel context failed") }
    context.draw(picture(row), in: CGRect(x: 0, y: 0, width: width, height: height))
    CVPixelBufferUnlockBaseAddress(pixels, [])
    guard adaptor.append(pixels, withPresentationTime: CMTime(seconds: presentation, preferredTimescale: 60_000)) else { fatalError("Frame append failed " + String(describing: writer.error)) }
}
input.markAsFinished()
let done = DispatchSemaphore(value: 0)
writer.finishWriting { done.signal() }
guard done.wait(timeout: .now() + 30) == .success, writer.status == .completed else { fatalError("Video not completed " + String(describing: writer.error)) }
let asset = AVURLAsset(url: destination)
let generator = AVAssetImageGenerator(asset: asset)
generator.appliesPreferredTrackTransform = true
let decoded = try generator.copyCGImage(at: .zero, actualTime: nil)
guard let png = CGImageDestinationCreateWithURL(URL(fileURLWithPath: args[3]) as CFURL, UTType.png.identifier as CFString, 1, nil) else { fatalError("Verification PNG failed") }
CGImageDestinationAddImage(png, decoded, nil)
guard CGImageDestinationFinalize(png) else { fatalError("PNG write failed") }
let report: [String: Any] = ["status": "ENCODED_AND_DECODED", "video": destination.path, "frames": rows.count, "width": width, "height": height, "lastPresentationSeconds": presentation, "cuts": cutCount, "audio": false, "scope": "Actual camera frames from the stated QA route, source timing retained within each segment; uncaptured travel/wait gaps cut. This motion sample is silent."]
let data = try JSONSerialization.data(withJSONObject: report, options: [.prettyPrinted, .sortedKeys])
try data.write(to: destination.appendingPathExtension("json"))
print(String(data: data, encoding: .utf8)!)
