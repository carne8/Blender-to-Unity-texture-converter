open System
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats

[<EntryPoint>]
let main args =
    match args |> Array.tryItem 0, args |> Array.tryItem 1 with
    | Some metallicImagePath, Some roughnessImagePath ->
        let metallic = Image.Load<Rgba32> metallicImagePath
        let roughness = Image.Load<Rgba32> roughnessImagePath

        let outputWidth = Math.Max(metallic.Width, roughness.Width)
        let outputHeight = Math.Max(metallic.Height, roughness.Height)
        let output = new Image<Rgba32>(outputWidth, outputHeight)

        output.ProcessPixelRows(
            metallic,
            roughness,
            (fun outputAccessor metallicAccessor roughnessAccessor ->
                for y = 0 to outputAccessor.Height - 1 do
                    let pixelRow = outputAccessor.GetRowSpan(y)
                    let metallicPixelRow = metallicAccessor.GetRowSpan(y)
                    let roughnessPixelRow = roughnessAccessor.GetRowSpan(y)

                    for x = 0 to pixelRow.Length - 1 do
                        let pixel = &pixelRow[x]
                        let metallicPixel = &metallicPixelRow[x]
                        let roughnessPixel = &roughnessPixelRow[x]

                        let r = metallicPixel.R
                        let g = metallicPixel.G
                        let b = metallicPixel.B
                        let a = 255uy - roughnessPixel.R
                        pixel <- new Rgba32(r, g, b, a))
        )

        let workingFolder = IO.Path.GetDirectoryName(metallicImagePath)
        let metallicImageFileName = IO.Path.GetFileNameWithoutExtension(metallicImagePath)
        let metallicImageFileExtension = IO.Path.GetExtension(metallicImagePath)
        let newMetallicImageFileName =
            sprintf "%s/%s without smoothness.%s"
                workingFolder
                metallicImageFileName
                metallicImageFileExtension

        IO.File.Move(metallicImagePath, newMetallicImageFileName, true)
        output.SaveAsPng metallicImagePath

        0
    | _ -> 1
